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
    public class ClientLocalInputFetch : IClientPredictorCorrections
    {
        private readonly Milliseconds fixedSimulationDeltaTimeMs;
        private readonly ILog log;
        private readonly TimeTicker fetchInputTicker;
        private readonly IEntityContainer world;
        private IInputPackFetch inputPackFetch;
        private readonly BundleAndSendOutInput bundleAndSendOutInput;
        private readonly Dictionary<byte, LocalPlayerInput> localPlayerInputs = new();
        private readonly ClientPredictor notifyPredictor;
        private bool usePrediction;

        private TickId inputTickId = new(1); // HACK: We need it to start ahead of the host

        public ClientLocalInputFetch(IInputPackFetch inputPackFetch, ClientPredictor notifyPredictor, bool usePrediction, ITransportClient transportClient,
            Milliseconds now, Milliseconds targetDeltaTimeMs, IEntityContainer world, ILog log)
        {
            this.log = log;
            this.world = world;
            this.notifyPredictor = notifyPredictor;
            this.usePrediction = usePrediction;
            this.inputPackFetch = inputPackFetch;
            fixedSimulationDeltaTimeMs = targetDeltaTimeMs;
            bundleAndSendOutInput = new(transportClient, log.SubLog("BundleInputAndSend"));
            fetchInputTicker = new(now, FetchAndStoreInputTick, targetDeltaTimeMs,
                log.SubLog("FetchAndStoreInputTick"));
        }

        public TickId LastSeenSnapshotTickId
        {
            set => bundleAndSendOutInput.LastSeenSnapshotTickId = value;
        }

        public TickId NextExpectedSnapshotTickId
        {
            set => bundleAndSendOutInput.NextExpectedSnapshotTickId = value;
        }

        public IInputPackFetch InputFetch
        {
            set => inputPackFetch = value;
        }

        public void AssignAvatarAndReadCorrections(TickId correctionsForTickId, ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);
            var snapshotReader = new OctetReader(physicsCorrectionPayload);

            var correctionsCount = snapshotReader.ReadUInt8();

            for (var i = 0; i < correctionsCount; ++i)
            {
                var (targetEntityId, localPlayerIndex, octetCount) = CorrectionsHeaderReader.Read(snapshotReader);
                var targetEntity = world.FetchEntity(targetEntityId);
                var wasFound = localPlayerInputs.TryGetValue(localPlayerIndex.Value, out var localPlayerInput);
                if (!wasFound || localPlayerInput is null)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, targetEntityId);
                    
                    //targetEntity  log.SubLog($"AvatarPredictor/{localPlayerIndex}")
                    localPlayerInput = new LocalPlayerInput(localPlayerIndex);
                    localPlayerInputs[localPlayerIndex.Value] = localPlayerInput;
                    notifyPredictor.CreateAvatarPredictor(localPlayerIndex, targetEntity);
                }

                //var changesThisSnapshot = targetEntity.CompleteEntity.Changes();

                var physicsCorrectionPayloadForLocalPlayer = snapshotReader.ReadOctets(octetCount);

                if (usePrediction)
                {
                    notifyPredictor.ReadCorrection(localPlayerIndex, correctionsForTickId,
                        physicsCorrectionPayloadForLocalPlayer);
                }
            }
        }

        public void AdjustInputTickSpeed(TickId lastReceivedServerTickId, uint roundTripTimeMs)
        {
            var targetPredictionTicks = roundTripTimeMs / fixedSimulationDeltaTimeMs.ms;

            var tickIdThatWeShouldSendNowInTheory = lastReceivedServerTickId.tickId + targetPredictionTicks;
            const int counterJitter = 2;
            const int counterProcessOrder = 1;
            var tickIdThatWeShouldSendNow = tickIdThatWeShouldSendNowInTheory + counterProcessOrder + counterJitter;

            var inputDiffInTicks = inputTickId.tickId - tickIdThatWeShouldSendNow;

            var newDeltaTimeMs = inputDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 100 / 120,
                > 30 => 0,
                > 0 => fixedSimulationDeltaTimeMs.ms * 100 / 80,
                _ => fixedSimulationDeltaTimeMs.ms
            };

            log.DebugLowLevel("New Input Fetch Speed {Diff} {NewDeltaTimeMs}", inputDiffInTicks, newDeltaTimeMs);

            fetchInputTicker.DeltaTime = new(newDeltaTimeMs);
        }

        public void Update(Milliseconds now)
        {
            fetchInputTicker.Update(now);
        }

        private void FetchAndStoreInputTick()
        {
            var now = fetchInputTicker.Now;

            log.Debug("--- Fetch And Store Input Tick {TickId}", inputTickId);

            var localPlayerInputsArray = localPlayerInputs.Values.ToArray();

            FetchAndStoreInput.FetchAndStore(inputTickId, inputPackFetch, localPlayerInputsArray,
                log);
            
            bundleAndSendOutInput.BundleAndSendInputDatagram(localPlayerInputsArray, now);

            if (usePrediction)
            {
                notifyPredictor.Predict(localPlayerInputsArray);
            }

            inputTickId = inputTickId.Next();
        }
    }
}