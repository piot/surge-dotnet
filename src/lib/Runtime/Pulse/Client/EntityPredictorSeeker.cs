/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class EntityPredictorSeeker
    {
        public static void Seek(EntityPredictor entityPredictor, TickId targetTickId, ILog log)
        {
            if (targetTickId < entityPredictor.FirstTickId)
            {
                log.Info("target tickId is too far away {TargetTickId} {RollbackStackId}", targetTickId,
                    entityPredictor.FirstTickId);
                return;
            }

            if (targetTickId > entityPredictor.LastTickId)
            {
                // We need to predict more
                log.Info("target tickId is too far away {TargetTickId} {RollbackStackEndId}", targetTickId,
                    entityPredictor.LastTickId);
                return;
            }

            log.Info("compare {TargetTickId} {CurrentTickId}", targetTickId, entityPredictor.PredictCollection.TickId);

            if (targetTickId > entityPredictor.PredictCollection.TickId)
            {
                log.Info("Predict to {TargetTickId}", targetTickId);
                Predict(entityPredictor, targetTickId);
            }
            else if (targetTickId == entityPredictor.PredictCollection.TickId)
            {
                log.Info("Replicate {TargetTickId}", targetTickId);
            }
            else
            {
                log.Info("Rollback to {TargetTickId}", targetTickId);
                Rollback(entityPredictor, targetTickId, log);
            }


            entityPredictor.DebugSetTickId(targetTickId);
        }

        public static void Predict(EntityPredictor entityPredictor, TickId targetTickId)
        {
            if (targetTickId > entityPredictor.LastTickId)
            {
                throw new("can not predict after last tick Id");
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
                //TODO:var inputDeserializeTarget = entityPredictor.AssignedAvatar.CompleteEntity as IInputDeserialize;
                var inputReader = new OctetReader(itemValue.inputPackSetBeforeThisTick.Span);
                //inputDeserializeTarget?.SetInput(inputReader);
                //entityPredictor.AssignedAvatar.CompleteEntity.Tick();
                //entityPredictor.AssignedAvatar.CompleteEntity.MovementSimulationTick();

                currentTickId = currentTickId.Next;
            }
        }

        public static void Rollback(EntityPredictor entityPredictor, TickId targetTickId, ILog log)
        {
            if (targetTickId < entityPredictor.FirstTickId)
            {
                throw new("can not go back before the beginning");
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

                log.Info("Rollback to {TickId}", currentTickId);
                RollBacker.RollBack(entityPredictor.AssignedAvatar, undoPack.Value.undoPack.Span);
                currentTickId = currentTickId.Previous;
            }
            // TOOD: entityPredictor.AssignedAvatar.CompleteEntity.FireReplicate();
        }
    }
}