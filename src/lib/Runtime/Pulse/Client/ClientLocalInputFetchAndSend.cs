/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Surge.Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Core;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LocalPlayer.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.TimeTick;
using Piot.Surge.Types.Serialization;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    /// <summary>
    ///     Predicts the future of an avatar. If misprediction occurs, it will roll back,
    ///     apply the correction, and fast forward (Roll forth).
    /// </summary>
    public sealed class ClientLocalInputFetchAndSend : IClientPredictorCorrections
    {
        readonly BundleAndSendOutInput bundleAndSendOutInput;
        readonly IDataSender toHostDataSender;
        readonly TimeTicker fetchInputTicker;
        readonly FixedDeltaTimeMs fixedSimulationDeltaTimeMs;
        readonly ILog log;
        readonly ClientPredictor notifyPredictor;
        readonly IDataReceiver world;
        bool weHaveReceivedInitialSnapshot = false;

        TickId inputTickId = new(1); // HACK: We need it to start ahead of the host

        public ClientLocalInputFetchAndSend(ClientPredictor notifyPredictor,
            bool usePrediction, ITransportClient transportClient,
            TimeMs now, FixedDeltaTimeMs targetDeltaTimeMs, IDataReceiver world, IDataSender toHostDataSender, ILog log)
        {
            this.log = log;
            this.world = world;
            this.notifyPredictor = notifyPredictor;
            UsePrediction = usePrediction;
            this.toHostDataSender = toHostDataSender;
            fixedSimulationDeltaTimeMs = targetDeltaTimeMs;
            bundleAndSendOutInput = new(transportClient, log.SubLog("BundleInputAndSend"));
            log.Info("target delta {time}", targetDeltaTimeMs);
            fetchInputTicker = new(now, FetchAndStoreInputTick, targetDeltaTimeMs,
                log.SubLog("FetchAndStoreInputTick"));

            var localPlayerIndex = new LocalPlayerIndex(1);
            var fakeAvatarPredictor = notifyPredictor.CreateAvatarPredictor(localPlayerIndex, new(53));
            LocalPlayerInputs.Add(localPlayerIndex.Value, new (localPlayerIndex, fakeAvatarPredictor, log));
            
            
        }

        public void StartPredictionFromTickId(TickId tickId)
        {
            log.Debug("Starting actual prediction input since we have received first snapshot");
            TickId = tickId;
            weHaveReceivedInitialSnapshot = true;
        }
        

        public TickId TickId
        {
            set => inputTickId = value;
            get => inputTickId;
        }

        public TickId LastSeenSnapshotTickId
        {
            set => bundleAndSendOutInput.LastSeenSnapshotTickId = value;
        }

        public TickId NextExpectedSnapshotTickId
        {
            set => bundleAndSendOutInput.NextExpectedSnapshotTickId = value;
        }

        public bool UsePrediction { set; get; }

        public bool ShouldStoreInputToPrediction { get; set; } = true;

        public Dictionary<byte, LocalPlayerInput> LocalPlayerInputs { get; } = new();




        public void AssignAvatarAndReadCorrections(TickId correctionsForTickId)
        {
//            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);
            if (LocalPlayerInputs.Values.Count > 0)
            {
                var firstLocalPlayer = LocalPlayerInputs.Values.First();
                if (firstLocalPlayer.AvatarPredictor.EntityPredictor.Count > 0)
                {
                    log.DebugLowLevel(
                        "we have corrections for {TickId}, clear old predicted inputs. Input in buffer {FirstTick} {LastTickId}",
                        correctionsForTickId,
                        firstLocalPlayer.AvatarPredictor.EntityPredictor.FirstTickId,
                        firstLocalPlayer.AvatarPredictor.EntityPredictor.LastTickId);
                }
            }

            foreach (var localPlayerInput in LocalPlayerInputs.Values)
            {
                if (ShouldStoreInputToPrediction)
                {
                    localPlayerInput.AvatarPredictor.EntityPredictor.DiscardUpToAndExcluding(
                        correctionsForTickId);
                }
            }

        }

        public void ReadPredictEntityIdsForLocalPlayers(IBitReader snapshotReader)
        {
            var localPlayerInformationCount = snapshotReader.ReadBits(4);

            for (var i = 0; i < localPlayerInformationCount; ++i)
            {
                var localPlayerIndex = LocalPlayerIndexReader.Read(snapshotReader);
                EntityIdReader.Read(snapshotReader, out var targetEntityId);
                
                var wasFound = LocalPlayerInputs.TryGetValue(localPlayerIndex.Value, out var localPlayerInput);
                if (!wasFound || localPlayerInput is null)
                {
                    log.Debug("assigned an avatar to {LocalPlayer} {EntityId}", localPlayerIndex, targetEntityId);

                    var createdPredictor = notifyPredictor.CreateAvatarPredictor(localPlayerIndex, targetEntityId);
                    localPlayerInput = new(localPlayerIndex, createdPredictor, log);
                    LocalPlayerInputs[localPlayerIndex.Value] = localPlayerInput;
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

            var inputDiffInTicks = (int)tickIdThatWeShouldSendNow - (int)inputTickId.tickId;

            var newDeltaTimeMs = inputDiffInTicks switch
            {
                < 0 => fixedSimulationDeltaTimeMs.ms * 110 / 100,
                > 0 => fixedSimulationDeltaTimeMs.ms * 60 / 100,
                _ => fixedSimulationDeltaTimeMs.ms
            };

            var maxInputCount = MaxPredictedInputQueueCount();
            if (maxInputCount > 32)
            {
                var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();
                foreach (var localPlayerInput in localPlayerInputsArray)
                {
                    localPlayerInput.AvatarPredictor.EntityPredictor.Reset();
                }
            }

            log.Debug(
                "New Input Fetch Speed. {LastReceivedServerTickId} {InputTickId} {TickIdThatWeShouldSendNow} {InputDiffInTicks} {NewDeltaTimeMs} based on {RoundTripTimeMs}",
                lastReceivedServerTickId, inputTickId.tickId, tickIdThatWeShouldSendNow, inputDiffInTicks, newDeltaTimeMs, roundTripTimeMs);

            fetchInputTicker.DeltaTime = new(newDeltaTimeMs);
        }

        int MaxPredictedInputQueueCount()
        {
            var maxInputCount = 0;
            var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();
            foreach (var localPlayerInput in localPlayerInputsArray)
            {
                if (localPlayerInput.AvatarPredictor.EntityPredictor.Count > maxInputCount)
                {
                    maxInputCount = localPlayerInput.AvatarPredictor.EntityPredictor.Count;
                }
            }

            return maxInputCount;
        }

        public void ResetTime(TimeMs now)
        {
            fetchInputTicker.Reset(now);
        }

        public void Update(TimeMs now)
        {
            fetchInputTicker.Update(now);
        }

        void FetchAndStoreInputTick()
        {
            var now = fetchInputTicker.Now;

            log.Debug("--- Fetch And Store Input Tick {TickId}", inputTickId);
            
            

            var localPlayerInputsArray = LocalPlayerInputs.Values.ToArray();

            if (localPlayerInputsArray.Length > 0)
            {
                log.Debug("Have {InputCount} in queue for first player",
                    localPlayerInputsArray[0].AvatarPredictor.EntityPredictor.PredictCollection.Count);
            }
            else
            {
                log.Debug("We have no local players that give input");
            }

            foreach (var localPlayerInput in localPlayerInputsArray)
            {
                if (localPlayerInput.AvatarPredictor.EntityPredictor.PredictCollection.Count > 50)
                {
                    log.Notice("Input queue is full, so we discard input");
                    return;
                }
            }
            
            log.Debug("Client Local Input Tick {InputTickId}", inputTickId);

            if (weHaveReceivedInitialSnapshot)
            {

                var inputsPackThisTick = InputComponentsSerializer.SerializeInputComponentsFromAssignedEntity(inputTickId, toHostDataSender, localPlayerInputsArray,
                    log);

                if (inputsPackThisTick.Length != localPlayerInputsArray.Length)
                {
                    throw new Exception("internal error, needs to have one input pack for each local input player");
                }

                log.Debug("--- Fetch And Store Input Tick result {LocalCount} {TickId} {InputsPackThisTick}", localPlayerInputsArray.Length, inputTickId, inputsPackThisTick);

                if (ShouldStoreInputToPrediction)
                {
                    notifyPredictor.Predict(localPlayerInputsArray, inputsPackThisTick, UsePrediction);
                }
            }

            bundleAndSendOutInput.BundleAndSendInputDatagram(localPlayerInputsArray, now, log);

            inputTickId = inputTickId.Next;
        }
    }
}