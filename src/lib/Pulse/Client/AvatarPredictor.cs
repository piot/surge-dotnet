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
                $"[AvatarPredictor localPlayer:{LocalPlayerIndex} entity:{EntityPredictor.AssignedAvatar.Id} predictedInputs:{EntityPredictor.Count}]";
        }

        public bool WeDidPredictTheFutureCorrectly(TickId correctionForTickId, ReadOnlySpan<byte> logicPayload,
            ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            if (EntityPredictor.PredictCollection.Count == 0)
            {
                // We have nothing to compare with, so lets assume it was correct
                return true;
            }

            var item =
                EntityPredictor.PredictCollection.FindFromTickId(correctionForTickId);

            if (item is null)
            {
                return true;
            }

            var v = item.Value;

            var encounteredLogicPayloadChecksum = Fnv.Fnv.ToFnv(logicPayload);
            var isLogicEqual = PredictionStateChecksum.IsEqual(v.logicStateFnvChecksum, v.logicStatePack.Span,
                logicPayload, encounteredLogicPayloadChecksum);
            if (!isLogicEqual)
            {
                log.Notice("Logic: {StateChecksum:x4} {StateLength} {EncounteredChecksum:x4} {EncounteredLength}",
                    v.logicStateFnvChecksum, v.logicStatePack.Length, encounteredLogicPayloadChecksum,
                    logicPayload.Length);
            }

            var encounteredPhysicsChecksum = Fnv.Fnv.ToFnv(physicsCorrectionPayload);
            var isPhysicsEqual = PredictionStateChecksum.IsEqual(v.physicsStateFnvChecksum,
                v.physicsStatePack.Span,
                physicsCorrectionPayload, encounteredPhysicsChecksum);
            if (!isPhysicsEqual)
            {
                log.Notice("Physics: {StateChecksum:x4} {StateLength} {EncounteredChecksum:x4} {EncounteredLength}",
                    v.physicsStateFnvChecksum, v.physicsStatePack.Length, encounteredPhysicsChecksum,
                    physicsCorrectionPayload.Length);
            }

            return isLogicEqual && isPhysicsEqual;
        }

        /// <summary>
        ///     Handles incoming correction state
        ///     If checksum is not the same, it rollbacks, replicate, and rollforth.
        /// </summary>
        public void ReadCorrection(TickId correctionForTickId, ReadOnlySpan<byte> physicsCorrectionPayload)
        {
            log.DebugLowLevel("got correction for {TickId}", correctionForTickId);
            EntityPredictor.DiscardUpToAndExcluding(correctionForTickId);

            var assignedAvatar = EntityPredictor.AssignedAvatar;
            var rollbackStack = EntityPredictor.PredictCollection;
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

            RollBacker.Rollback(assignedAvatar, rollbackStack, rollbackStack.FirstTickId, correctionForTickId, log);

            log.DebugLowLevel("Replicating new fresh state to {TickId}", correctionForTickId);
            Replicator.Replicate(assignedAvatar, logicNowReplicateWriter.Octets, physicsCorrectionPayload);

            if (shouldPredict)
            {
                EntityPredictor.CachedUndoWriter.Reset();
                RollForth.Rollforth(assignedAvatar, EntityPredictor.PredictCollection,
                    EntityPredictor.CachedUndoWriter, log);
            }

            assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;
        }
    }
}