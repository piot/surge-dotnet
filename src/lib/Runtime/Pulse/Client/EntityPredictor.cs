/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public class EntityPredictor
    {
        readonly ILog log;

        public EntityPredictor(EntityId assignedAvatar, ILog log)
        {
            AssignedAvatar = assignedAvatar;
            this.log = log;
        }

        public EntityId AssignedAvatar { get; }

        public PredictCollection PredictCollection { get; } = new();

        public OctetWriter CachedUndoWriter { get; } = new(1024);

        public TickId TickId => PredictCollection.TickId;
        public TickId FirstTickId => PredictCollection.FirstTickId;
        public TickId LastTickId => PredictCollection.LastTickId;

        public PredictItem? LastItem => PredictCollection.LastItem;

        public int Count => PredictCollection.Count;

        public void DebugSetTickId(TickId tickId)
        {
            PredictCollection.DebugSetTickId(tickId);
        }

        public void Reset()
        {
            log.Warn("RESET entity predictor");
            PredictCollection.Reset();
        }

        public void DiscardUpToAndExcluding(TickId correctionForTickId)
        {
            log.DebugLowLevel("discard up to {correctionForTickId}", correctionForTickId);
            PredictCollection.DiscardUpToAndExcluding(correctionForTickId);
        }

        public void AddInput(LogicalInput.LogicalInput logicalInput, bool doActualPrediction)
        {
            // TODO: AssignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;

            /* TODO:
             if (AssignedAvatar.CompleteEntity is not IInputDeserialize inputTarget)
            {
                throw new(
                    $"It is not possible to control Entity {AssignedAvatar.Id}, it has no IDeserializeInput interface");
            }
            */

            var inputReader = new OctetReader(logicalInput.payload.Span);
            if (doActualPrediction)
            {
                // TODO:  inputTarget.SetInput(inputReader);
            }

            log.Info("Set input so it can result in {TickId}", logicalInput.appliedAtTickId.Next);

            CachedUndoWriter.Reset();
            PredictAndSaver.PredictAndSave(AssignedAvatar, PredictCollection, logicalInput, CachedUndoWriter,
                PredictMode.Predicting, doActualPrediction);
        }
    }
}