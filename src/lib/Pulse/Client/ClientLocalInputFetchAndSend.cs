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
    public sealed class ClientLocalInputFetchAndSend : IClientPredictorCorrections
    {
        private readonly BundleAndSendOutInput bundleAndSendOutInput;
        private readonly TimeTicker fetchInputTicker;
        private readonly FixedDeltaTimeMs fixedSimulationDeltaTimeMs;
        private readonly ILog log;
        private readonly ClientPredictor notifyPredictor;
        private readonly bool usePrediction;
        private readonly IEntityContainer world;
        private IInputPackFetch inputPackFetch;

        private TickId inputTickId = new(1); // HACK: We need it to start ahead of the host

        public ClientLocalInputFetchAndSend(IInputPackFetch inputPackFetch, ClientPredictor notifyPredictor,
            bool usePrediction, ITransportClient transportClient,
            TimeMs now, FixedDeltaTimeMs targetDeltaTimeMs, IEntityContainer world, ILog log)
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

        public void ReadAndAssignLocalPlayers(ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            ReadCorrectionsAndAssignLocalPlayerInput(physicsCorrectionPayload);
        }

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

            foreach (var localPlayerInput in LocalPlayerInputs.Values)
            {
                localPlayerInput.PredictedInputs.DiscardUpToAndExcluding(correctionsForTickId);
            }

            var corrections = ReadCorrectionsAndAssignLocalPlayerInput(physicsCorrectionPayload);

            foreach (var correction in corrections)
            {
                if (correction.wasCreatedNow)
                {
                    notifyPredictor.CreateAvatarPredictor(correction.localPlayerInput);
                }

                if (usePrediction)
                {
                    notifyPredictor.ReadCorrection(correction.localPlayerInput.LocalPlayerIndex, correctionsForTickId,
                        correction.payload.Span);
                }
            }
        }

        public CorrectionInfo[] ReadCorrectionsAndAssignLocalPlayerInput(ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            var snapshotReader = new OctetReader(physicsCorrectionPayload);
            var correctionsCount = snapshotReader.ReadUInt8();

            var corrections = new List<CorrectionInfo>();

            for (var i = 0; i < correctionsCount; ++i)
            {
                var (targetEntityId, localPlayerIndex, octetCount) = CorrectionsHeaderReader.Read(snapshotReader);
                var wasFound = LocalPlayerInputs.TryGetValue(localPlayerIndex.Value, out var localPlayerInput);
                var wasCreatedNow = false;
                if (!wasFound || localPlayerInput is null)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, targetEntityId);
                    var targetEntity = world.FetchEntity(targetEntityId);

                    localPlayerInput = new LocalPlayerInput(localPlayerIndex, targetEntity);
                    LocalPlayerInputs[localPlayerIndex.Value] = localPlayerInput;
                    wasCreatedNow = true;
                }

                var physicsCorrectionPayloadForLocalPlayer = snapshotReader.ReadOctets(octetCount);

                var correctionInfo = new CorrectionInfo
                {
                    wasCreatedNow = wasCreatedNow,
                    localPlayerInput = localPlayerInput,
                    payload = physicsCorrectionPayloadForLocalPlayer.ToArray()
                };

                corrections.Add(correctionInfo);
            }

            return corrections.ToArray();
        }

        public void AdjustInputTickSpeed(TickId lastReceivedServerTickId, uint roundTripTimeMs)
        {
            var targetPredictionTicks = roundTripTimeMs / fixedSimulationDeltaTimeMs.ms;

            var tickIdThatWeShouldSendNowInTheory = lastReceivedServerTickId.tickId + targetPredictionTicks;
            const int counterJitter = 2;
            const int counterProcessOrder = 1;
            var tickIdThatWeShouldSendNow = tickIdThatWeShouldSendNowInTheory + counterProcessOrder + counterJitter;

            var inputDiffInTicks = (int)tickIdThatWeShouldSendNow - (int)inputTickId.tickId;

            var newDeltaTimeMs = inputDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 110 / 100,
                > 0 => fixedSimulationDeltaTimeMs.ms * 90 / 100,
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
                "New Input Fetch Speed {tickId} {TickIdThatWeShouldSendNow} {InputDiffInTicks} {NewDeltaTimeMs} based on {RoundTripTimeMs}",
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

        public void Update(TimeMs now)
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
                log.DebugLowLevel("Have {InputCount} in queue for first player",
                    localPlayerInputsArray[0].PredictedInputs.Collection.Length);
            }
            else
            {
                log.DebugLowLevel("We have no local players that give input");
            }

            FetchAndStoreInput.FetchAndStore(inputTickId, inputPackFetch, localPlayerInputsArray,
                log);

            bundleAndSendOutInput.BundleAndSendInputDatagram(localPlayerInputsArray, now);

            if (usePrediction)
            {
                notifyPredictor.Predict(localPlayerInputsArray);
            }

            inputTickId = inputTickId.Next;
        }

        public struct CorrectionInfo
        {
            public bool wasCreatedNow;
            public LocalPlayerInput localPlayerInput;

            public Memory<byte> payload;
        }
    }
}