/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Corrections.Serialization;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInput.Serialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Predicts the future of an avatar. If misprediction occurs, it will roll back,
    ///     apply the correction, and fast forward (Roll forth).
    /// </summary>
    public class ClientPredictor : IClientPredictorCorrections
    {
        private readonly OrderedDatagramsSequenceIdIncrease datagramsOut = new();
        private readonly Milliseconds fixedSimulationDeltaTimeMs;
        private readonly Dictionary<byte, AvatarPredictor> localAvatarPredictors = new();
        private readonly ILog log;
        private readonly TimeTicker predictionTicker;
        private readonly ITransportClient transportClient;
        private readonly IEntityContainer world;
        private IInputPackFetch inputPackFetch;
        private TickId lastSeenSnapshotTickId;

        private TickId nextExpectedSnapshotTickId;
        private TickId predictTickId = new(1); // HACK: We need it to start ahead of the host

        public ClientPredictor(IInputPackFetch inputPackFetch, ITransportClient transportClient,
            Milliseconds now, Milliseconds targetDeltaTimeMs, IEntityContainer world,
            ILog log)
        {
            this.log = log;
            this.world = world;
            this.transportClient = transportClient;
            this.inputPackFetch = inputPackFetch;
            fixedSimulationDeltaTimeMs = targetDeltaTimeMs;
            predictionTicker = new(now, PredictionTick, targetDeltaTimeMs,
                log.SubLog("PredictionTick"));
        }

        public TickId LastSeenSnapshotTickId
        {
            set => lastSeenSnapshotTickId = value;
        }

        public TickId NextExpectedSnapshotTickId
        {
            set => nextExpectedSnapshotTickId = value;
        }

        public IInputPackFetch InputFetch
        {
            set => inputPackFetch = value;
        }

        public void ReadCorrections(TickId correctionsForTickId, ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);
            var snapshotReader = new OctetReader(physicsCorrectionPayload);

            var correctionsCount = snapshotReader.ReadUInt8();

            for (var i = 0; i < correctionsCount; ++i)
            {
                var (targetEntityId, localPlayerIndex, octetCount) = CorrectionsHeaderReader.Read(snapshotReader);
                var targetEntity = world.FetchEntity(targetEntityId);
                var wasFound = localAvatarPredictors.TryGetValue(localPlayerIndex.Value, out var predictor);
                if (!wasFound || predictor is null)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, targetEntityId);
                    predictor = new AvatarPredictor(localPlayerIndex, targetEntity,
                        log.SubLog($"AvatarPredictor/{localPlayerIndex}"));
                    localAvatarPredictors[localPlayerIndex.Value] = predictor;
                }

                //var changesThisSnapshot = targetEntity.GeneratedEntity.Changes();

                var physicsCorrectionPayloadForLocalPlayer = snapshotReader.ReadOctets(octetCount);

                predictor.ReadCorrection(correctionsForTickId, physicsCorrectionPayloadForLocalPlayer);
            }
        }


        public void AdjustPredictionSpeed(TickId lastReceivedServerTickId, uint roundTripTimeMs)
        {
            var targetPredictionTicks = roundTripTimeMs / fixedSimulationDeltaTimeMs.ms;

            var tickIdThatWeShouldSendNowInTheory = lastReceivedServerTickId.tickId + targetPredictionTicks;
            const int counterJitter = 2;
            const int counterProcessOrder = 1;
            var tickIdThatWeShouldSendNow = tickIdThatWeShouldSendNowInTheory + counterProcessOrder + counterJitter;

            var predictionDiffInTicks = predictTickId.tickId - tickIdThatWeShouldSendNow;

            var newDeltaTimeMs = predictionDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 100 / 120,
                > 30 => 0,
                > 0 => fixedSimulationDeltaTimeMs.ms * 100 / 80,
                _ => fixedSimulationDeltaTimeMs.ms
            };

            log.DebugLowLevel("New Prediction Speed {Diff} {NewDeltaTimeMs}", predictionDiffInTicks, newDeltaTimeMs);

            predictionTicker.DeltaTime = new(newDeltaTimeMs);
        }

        public void Update(Milliseconds now)
        {
            predictionTicker.Update(now);
        }

        private void PredictionTick()
        {
            var now = predictionTicker.Now;

            log.Debug("--- Prediction Tick {TickId}", predictTickId);

            var usePrediction = false;

            var localAvatarPredictorsArray = localAvatarPredictors.Values.ToArray();

            PredictLocalAvatars.PredictLocalPlayers(predictTickId, inputPackFetch, localAvatarPredictorsArray,
                usePrediction, log);

            var logicalInputForAllPlayers =
                LocalPlayerLogicalInputBundler.BundleInputForAllLocalPlayers(localAvatarPredictorsArray);

            var droppedSnapshotCount = lastSeenSnapshotTickId > nextExpectedSnapshotTickId
                ? (byte)(lastSeenSnapshotTickId - nextExpectedSnapshotTickId).tickId
                : (byte)0;
            var outDatagram =
                LogicInputDatagramPackOut.CreateInputDatagram(datagramsOut.Value, nextExpectedSnapshotTickId,
                    droppedSnapshotCount,
                    now, logicalInputForAllPlayers);

            log.DebugLowLevel("Sending inputs to host {FirstTickId} {LastTickId}",
                logicalInputForAllPlayers.debugFirstId, logicalInputForAllPlayers.debugLastId);

            transportClient.SendToHost(outDatagram);

            datagramsOut.Increase();
            predictTickId = predictTickId.Next();
        }
    }
}