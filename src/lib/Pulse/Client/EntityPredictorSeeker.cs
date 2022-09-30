/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class EntityPredictorSeeker
    {
        public static void Seek(EntityPredictor entityPredictor, TickId targetTickId, ILog log)
        {
            if (targetTickId < entityPredictor.FirstTickId)
            {
                log.Debug("target tickId is too far away {TargetTickId} {RollbackStackId}", targetTickId,
                    entityPredictor.FirstTickId);
                return;
            }

            if (targetTickId > entityPredictor.LastTickId)
            {
                // We need to predict more
                log.Debug("target tickId is too far away {TargetTickId} {RollbackStackEndId}", targetTickId,
                    entityPredictor.LastTickId);
                return;
            }

            if (targetTickId > entityPredictor.PredictCollection.TickId)
            {
                Predict(entityPredictor, targetTickId);
            }
            else
            {
                Rollback(entityPredictor, targetTickId);
            }
        }

        public static void Predict(EntityPredictor entityPredictor, TickId targetTickId)
        {
            if (targetTickId > entityPredictor.LastTickId)
            {
                return;
            }

            if (targetTickId <= entityPredictor.TickId)
            {
                throw new("can not predict because target is less than current, we should rollback instead");
            }

            var currentTickId = entityPredictor.PredictCollection.TickId;

            while (currentTickId < targetTickId)
            {
                var item = entityPredictor.PredictCollection.FindFromTickId(currentTickId);
                if (item is null)
                {
                    throw new("should have an item");
                    return;
                }

                var itemValue = item.Value;
                var temporaryInput = new LogicalInput.LogicalInput(new(0), itemValue.tickId,
                    itemValue.inputPackSetBeforeThisTick.Span);
                entityPredictor.AddInput(temporaryInput, true);
                currentTickId = currentTickId.Next;
            }
        }

        public static void Rollback(EntityPredictor entityPredictor, TickId targetTickId)
        {
            if (targetTickId > entityPredictor.LastTickId)
            {
                return;
            }

            if (targetTickId >= entityPredictor.TickId)
            {
                throw new("can not go back in rollback because it is not less than current, we should predict instead");
            }

            var currentTickId = entityPredictor.PredictCollection.TickId;

            while (currentTickId < targetTickId)
            {
                var undoPack = entityPredictor.PredictCollection.FindFromTickId(currentTickId);
                if (undoPack is null)
                {
                    throw new InvalidOperationException($"could not find undo pack for {currentTickId}");
                }

                RollBacker.RollBack(entityPredictor.AssignedAvatar, undoPack.Value.undoPack.Span);
                currentTickId = currentTickId.Previous;
            }
        }
    }
}