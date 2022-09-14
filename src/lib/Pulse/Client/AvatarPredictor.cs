/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public sealed class AvatarPredictor
    {
        private readonly IEntity assignedAvatar;
        private readonly OctetWriter cachedUndoWriter = new(1024);
        private readonly ILog log;

        private readonly PredictionStateChecksumQueue predictionStateChecksumHistory = new();
        private readonly RollbackStack rollbackStack = new();
        private readonly bool shouldPredictGoingForward = true;
        private bool shouldPredict = true;

        public AvatarPredictor(LocalPlayerIndex localPlayerIndex, IEntity assignedAvatar, ILog log)
        {
            this.log = log;
            LocalPlayerInput = new(localPlayerIndex, assignedAvatar);
            this.assignedAvatar = assignedAvatar;
        }

        public LocalPlayerInput LocalPlayerInput { get; }

        public override string ToString()
        {
            return
                $"[AvatarPredictor localPlayer:{LocalPlayerInput.LocalPlayerIndex} entity:{assignedAvatar.Id} predictedInputs:{LocalPlayerInput.PredictedInputs.Count}]";
        }

        public bool WeDidPredictTheFutureCorrectly(TickId correctionForTickId, ReadOnlySpan<byte> logicPayload,
            ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            if (predictionStateChecksumHistory.Count == 0)
            {
                // We have nothing to compare with, so lets assume it was correct
                return true;
            }

            var storedPredictionStateChecksum =
                predictionStateChecksumHistory.DequeueForTickId(correctionForTickId);
            return storedPredictionStateChecksum.IsEqual(logicPayload, physicsCorrectionPayload);
        }

        /// <summary>
        ///     Handles incoming correction state
        ///     If checksum is not the same, it rollbacks, replicate, and rollforth.
        /// </summary>
        /// <param name="correctionsForTickId"></param>
        /// <param name="snapshotReader"></param>
        /// <param name="world"></param>
        /// <param name="log"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ReadCorrection(TickId correctionForTickId, ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            LocalPlayerInput.PredictedInputs.DiscardUpToAndExcluding(correctionForTickId);

            log.DebugLowLevel("CorrectionsHeader {EntityId}, {LocalPlayerIndex} {Checksum}", assignedAvatar.Id,
                LocalPlayerInput.LocalPlayerIndex, physicsCorrectionPayload.Length);

            var logicNowReplicateWriter = new OctetWriter(1024);
            var changesThisSnapshot = assignedAvatar.CompleteEntity.Changes();
            assignedAvatar.CompleteEntity.Serialize(changesThisSnapshot, logicNowReplicateWriter);

            if (!shouldPredictGoingForward)
            {
                shouldPredict = false;
            }

            var logicNowWriter = new OctetWriter(1024);
            assignedAvatar.CompleteEntity.SerializeAll(logicNowWriter);

            if (WeDidPredictTheFutureCorrectly(correctionForTickId, logicNowWriter.Octets, physicsCorrectionPayload))
            {
                return;
            }

            log.Notice("Mis-predict at {TickId} for entity {EntityId}", correctionForTickId, assignedAvatar.Id);

            RollBacker.Rollback(assignedAvatar, rollbackStack, correctionForTickId);
            Replicator.Replicate(assignedAvatar, logicNowReplicateWriter.Octets, physicsCorrectionPayload);

            if (shouldPredict)
            {
                cachedUndoWriter.Reset();
                RollForth.Rollforth(assignedAvatar, LocalPlayerInput.PredictedInputs, rollbackStack,
                    predictionStateChecksumHistory, cachedUndoWriter);
            }

            assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;
        }


        public void Predict(LogicalInput.LogicalInput logicalInput)
        {
            assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;

            if (assignedAvatar.CompleteEntity is not IInputDeserialize inputDeserialize)
            {
                throw new Exception(
                    $"It is not possible to control Entity {assignedAvatar.Id}, it has no IDeserializeInput interface");
            }

            var inputReader = new OctetReader(logicalInput.payload.Span);
            inputDeserialize.SetInput(inputReader);
            cachedUndoWriter.Reset();
            PredictionTickAndStateSave.PredictAndStateSave(assignedAvatar, logicalInput.appliedAtTickId, rollbackStack,
                predictionStateChecksumHistory, PredictMode.Predicting, cachedUndoWriter);
        }
    }
}