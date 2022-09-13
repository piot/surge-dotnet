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
        private readonly BundleAndSendOutInput bundleAndSendOutInput;
        private readonly TimeTicker fetchInputTicker;
        private readonly Milliseconds fixedSimulationDeltaTimeMs;
        private readonly ILog log;
        private readonly ClientPredictor notifyPredictor;
        private readonly bool usePrediction;
        private readonly IEntityContainer world;
        private IInputPackFetch inputPackFetch;

        private TickId inputTickId = new(1); // HACK: We need it to start ahead of the host

        public ClientLocalInputFetch(IInputPackFetch inputPackFetch, ClientPredictor notifyPredictor,
            bool usePrediction, ITransportClient transportClient,
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

        public Dictionary<byte, LocalPlayerInput> LocalPlayerInputs { get; } = new();

        public void AssignAvatarAndReadCorrections(TickId correctionsForTickId,
            ReadOnlySpan<byte> physicsCorrectionPayload)
        {
//            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);
            if (LocalPlayerInputs.Values.Count > 0)
            {
                var firstLocalPlayer = LocalPlayerInputs.Values.First();
                if (firstLocalPlayer.PredictedInputs.Count > 0)
                {
                    log.DebugLowLevel(
                        "we have corrections for {TickId}, clear old predicted inputs. Input in buffer {FirstTick} {LastTickId}",
                        correctionsForTickId, firstLocalPlayer.PredictedInputs.Peek().appliedAtTickId,
                        firstLocalPlayer.PredictedInputs.Last.appliedAtTickId);
                }
            }

            var snapshotReader = new OctetReader(physicsCorrectionPayload);

            var correctionsCount = snapshotReader.ReadUInt8();

            foreach (var localPlayerInput in LocalPlayerInputs.Values)
            {
                localPlayerInput.PredictedInputs.DiscardUpToAndExcluding(correctionsForTickId);
            }

            for (var i = 0; i < correctionsCount; ++i)
            {
                var (targetEntityId, localPlayerIndex, octetCount) = CorrectionsHeaderReader.Read(snapshotReader);
                var targetEntity = world.FetchEntity(targetEntityId);
                var wasFound = LocalPlayerInputs.TryGetValue(localPlayerIndex.Value, out var localPlayerInput);
                if (!wasFound || localPlayerInput is null)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, targetEntityId);

                    //targetEntity  log.SubLog($"AvatarPredictor/{localPlayerIndex}")
                    localPlayerInput = new LocalPlayerInput(localPlayerIndex, targetEntity);
                    LocalPlayerInputs[localPlayerIndex.Value] = localPlayerInput;
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

            var inputDiffInTicks = tickIdThatWeShouldSendNow - inputTickId.tickId;

            var newDeltaTimeMs = inputDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 190 / 100,
                > 0 => fixedSimulationDeltaTimeMs.ms * 70 / 100,
                _ => fixedSimulationDeltaTimeMs.ms
            };

            var maxInputCount = MaxPredictedInputQueueCount();
            if (maxInputCount > 25)
            {
                var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();
                foreach (var localPlayerInput in localPlayerInputsArray)
                {
                    localPlayerInput.PredictedInputs.Reset();
                }
            }

            log.Debug(
                "New Input Fetch Speed {Diff} {tickId} {TickIdThatWeShouldSendNow} {NewDeltaTimeMs} based on {RoundTripTimeMs}",
                inputTickId.tickId, tickIdThatWeShouldSendNow, inputDiffInTicks, newDeltaTimeMs, roundTripTimeMs);

            fetchInputTicker.DeltaTime = new(newDeltaTimeMs);
        }

        private int MaxPredictedInputQueueCount()
        {
            var maxInputCount = 0;
            var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();
            foreach (var localPlayerInput in localPlayerInputsArray)
            {
                if (localPlayerInput.PredictedInputs.Count > maxInputCount)
                {
                    maxInputCount = localPlayerInput.PredictedInputs.Count;
                }
            }

            return maxInputCount;
        }

        public void Update(Milliseconds now)
        {
            fetchInputTicker.Update(now);
        }

        private void FetchAndStoreInputTick()
        {
            var now = fetchInputTicker.Now;

            log.Debug("--- Fetch And Store Input Tick {TickId}", inputTickId);

            var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();

            if (localPlayerInputsArray.Length > 0)
            {
                log.DebugLowLevel("Have {InputCount} in queue",
                    localPlayerInputsArray[0].PredictedInputs.Collection.Length);
            }

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