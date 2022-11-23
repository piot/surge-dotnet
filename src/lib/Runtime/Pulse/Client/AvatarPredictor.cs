/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Text;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public sealed class AvatarPredictor
    {
        readonly ILog log;
        readonly bool shouldPredictGoingForward = true;
        bool shouldPredict = true;

        public AvatarPredictor(uint debugIndex, EntityId assignedAvatar, ILog log)
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
                $"[AvatarPredictor localPlayer:{LocalPlayerIndex} entity:{EntityPredictor.AssignedAvatar} predictedInputs:{EntityPredictor.Count}]";
        }

        public PredictItem? FetchItemFromTickId(TickId correctionForTickId)
        {
            if (EntityPredictor.PredictCollection.Count == 0)
            {
                // We have nothing to compare with, so lets assume it was correct
                return null;
            }

            var item =
                EntityPredictor.PredictCollection.FindFromTickId(correctionForTickId);

            return item;
        }

        public bool WeDidPredictTheFutureCorrectly(PredictItem v, ReadOnlySpan<byte> logicPayload,
            ReadOnlySpan<byte> physicsCorrectionPayload)
        {
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



        public static string OctetsToString(ReadOnlySpan<byte> ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
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
            log.DebugLowLevel("CorrectionsHeader {EntityId}, {LocalPlayerIndex} {Checksum}", assignedAvatar,
                LocalPlayerIndex, physicsCorrectionPayload.Length);

            /*
            var logicBeforeOnlyChanges = new OctetWriter(1024);
            var changesThisSnapshot = assignedAvatar.CompleteEntity.Changes();
            assignedAvatar.CompleteEntity.Serialize(changesThisSnapshot, logicBeforeOnlyChanges);
            */

            var correctedFullSerialization = new OctetWriter(1024);
            //TODO: assignedAvatar.CompleteEntity.SerializeAll(correctedFullSerialization);

            if (!shouldPredictGoingForward)
            {
                shouldPredict = false;
            }

            // var before = EntityToDebugString(assignedAvatar);
            //var octetsBefore = OctetsToString(correctedFullSerialization.Octets);

            var predictItem = FetchItemFromTickId(correctionForTickId);
            if (predictItem is null)
            {
                // We haven't predicted this earlier, so just return
                return;
            }

            var v = predictItem.Value;

            if (correctedFullSerialization.Octets.Length != v.logicStatePack.Length)
            {
                log.Info($"complete {correctedFullSerialization.Octets.Length} {v.logicStatePack.Length}");
            }

            if (WeDidPredictTheFutureCorrectly(v, correctedFullSerialization.Octets, physicsCorrectionPayload))
            {
                var reader = new OctetReader(EntityPredictor.LastItem!.Value.logicStatePack.Span);
                // TODO: assignedAvatar.CompleteEntity.DeserializeAll(reader);
                return;
            }

            var predictedOctetsString = OctetsToString(v.logicStatePack.Span);
            var correctionOctetsString = OctetsToString(correctedFullSerialization.Octets);

            var setToOldPredictionReader = new OctetReader(v.logicStatePack.Span);
            // TODO: assignedAvatar.CompleteEntity.DeserializeAll(setToOldPredictionReader);
            //var predictedEntityString = EntityToDebugString(assignedAvatar);

            log.Notice("Mis-predict at {TickId} for entity {EntityId}", correctionForTickId, assignedAvatar);
            log.Notice("Mis-predict predicted {BeforeLocal} for correction {Correction}", predictedOctetsString,
                correctionOctetsString);


            var setToLastStateReader = new OctetReader(EntityPredictor.LastItem!.Value.logicStatePack.Span);
            // TODO: assignedAvatar.CompleteEntity.DeserializeAll(setToLastStateReader);

            #if true
            RollBacker.Rollback(assignedAvatar, rollbackStack, rollbackStack.TickId, correctionForTickId, log);

            log.DebugLowLevel("Replicating new fresh state to {TickId}", correctionForTickId);
            //Replicator.Replicate(assignedAvatar, correctedFullSerialization.Octets, physicsCorrectionPayload);
            //var replicate = EntityToDebugString(assignedAvatar);
            //log.Notice("Mis-predict entity {before} for entity {Correction}", predictedEntityString, replicate);
            if (shouldPredict)
            {
                EntityPredictor.CachedUndoWriter.Reset();
                RollForth.Rollforth(assignedAvatar, EntityPredictor.PredictCollection,
                    EntityPredictor.CachedUndoWriter, log);
            }
#endif
            // TODO: assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;
        }
    }
}