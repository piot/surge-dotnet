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
        readonly ILog log;
        readonly bool shouldPredictGoingForward = true;
        bool shouldPredict = true;

        public AvatarPredictor(uint debugIndex, IEntity assignedAvatar, ILog log)
        {
            this.log = log;
            LocalPlayerIndex = debugIndex;
            EntityPredictor = new(assignedAvatar, log.SubLog("EntityPredictor"));
        }

        public uint LocalPlayerIndex { get; }

        public EntityPredictor EntityPredictor { get; }

        public override string ToString()
        {
            return
                $"[AvatarPredictor localPlayer:{LocalPlayerIndex} entity:{EntityPredictor.AssignedAvatar.Id} predictedInputs:{EntityPredictor.PredictedInputs.Count}]";
        }

        public bool WeDidPredictTheFutureCorrectly(TickId correctionForTickId, ReadOnlySpan<byte> logicPayload,
            ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            if (EntityPredictor.PredictionStateHistory.Count == 0)
            {
                // We have nothing to compare with, so lets assume it was correct
                return true;
            }

            var storedPredictionStateChecksum =
                EntityPredictor.PredictionStateHistory.DequeueForTickId(correctionForTickId);

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
            log.Info("got correction for {TickId}", correctionForTickId);
            EntityPredictor.DiscardUpToAndExcluding(correctionForTickId);

            var assignedAvatar = EntityPredictor.AssignedAvatar;
            var rollbackStack = EntityPredictor.RollbackStack;
            log.DebugLowLevel("CorrectionsHeader {EntityId}, {LocalPlayerIndex} {Checksum}", assignedAvatar.Id,
                LocalPlayerIndex, physicsCorrectionPayload.Length);

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
                EntityPredictor.UndoWriter.Reset();
                RollForth.Rollforth(assignedAvatar, EntityPredictor.PredictedInputs, rollbackStack,
                    EntityPredictor.PredictionStateHistory, EntityPredictor.UndoWriter);
            }

            assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;
        }
    }
}