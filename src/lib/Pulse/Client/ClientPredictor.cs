/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
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
        private readonly OrderedDatagramsOut datagramsOut;
        private readonly Milliseconds fixedSimulationDeltaTimeMs;
        private readonly IInputPackFetch inputPackFetch;
        private readonly Dictionary<byte, AvatarPredictor> localAvatarPredictors = new();
        private readonly ILog log;
        private readonly TimeTicker predictionTicker;
        private readonly ITransportClient transportClient;
        private readonly IEntityContainer world;
        private IMultiCompressor compression;
        private TickId predictTickId;

        public ClientPredictor(IInputPackFetch inputPackFetch, ITransportClient transportClient,
            IMultiCompressor compression, Milliseconds now,
            Milliseconds targetDeltaTimeMs, IEntityContainer world,
            ILog log)
        {
            this.compression = compression;
            this.log = log;
            this.world = world;
            this.transportClient = transportClient;
            this.inputPackFetch = inputPackFetch;
            fixedSimulationDeltaTimeMs = targetDeltaTimeMs;
            predictionTicker = new(now, PredictionTick, targetDeltaTimeMs,
                log.SubLog("PredictionTick"));
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

            foreach (var localAvatarPredictor in localAvatarPredictors.Values)
            {
                log.Debug("--- Prediction Tick {TickId}", predictTickId);
                var inputOctets = inputPackFetch.Fetch(localAvatarPredictor.LocalPlayerIndex);
                var logicalInput = new LogicalInput.LogicalInput
                {
                    appliedAtTickId = predictTickId,
                    payload = inputOctets.ToArray()
                };

                log.DebugLowLevel("Adding logical input {LogicalInput}", logicalInput);
                localAvatarPredictor.PredictedInputs.AddLogicalInput(logicalInput);
                localAvatarPredictor.Predict(logicalInput);
            }

            var inputForAllPlayers = new LogicalInputArrayForPlayer[localAvatarPredictors.Count];
            var index = 0;
            foreach (var localAvatarPredictor in localAvatarPredictors.Values)
            {
                var inputForLocal = localAvatarPredictor.PredictedInputs.Collection;
                inputForAllPlayers[index].inputForEachPlayerInSequence = inputForLocal;
                index++;
            }

            var outDatagram =
                LogicInputDatagramPackOut.CreateInputDatagram(datagramsOut, new TickId(42), 0,
                    now, new LogicalInputsForAllLocalPlayers(inputForAllPlayers));
            log.DebugLowLevel("Sending inputs to host");
            transportClient.SendToHost(outDatagram);

            predictTickId = new TickId(predictTickId.tickId + 1);
        }
    }
}