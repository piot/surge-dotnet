/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public sealed class AvatarPredictor
    {
        readonly IEntity assignedAvatar;
        readonly OctetWriter cachedUndoWriter = new(1024);
        readonly ILog log;

        readonly PredictionStateChecksumQueue predictionStateChecksumHistory = new();
        readonly RollbackStack rollbackStack = new();
        readonly bool shouldPredictGoingForward = true;
        bool shouldPredict = true;

        public AvatarPredictor(LocalPlayerInput localPlayerInput, IEntity assignedAvatar, ILog log)
        {
            this.log = log;
            LocalPlayerInput = localPlayerInput;
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

            if (storedPredictionStateChecksum is null)
            {
                return true;
            }

            return storedPredictionStateChecksum.Value.IsEqual(logicPayload, physicsCorrectionPayload);
        }

        /// <summary>
        ///     Handles incoming correction state
        ///     If checksum is not the same, it rollbacks, replicate, and rollforth.
        /// </summary>
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
                throw new(
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