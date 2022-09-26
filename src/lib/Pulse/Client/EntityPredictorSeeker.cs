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
                log.Debug("target tickId is too far away {TargetTickId} {RollbackStackId}", targetTickId, entityPredictor.FirstTickId);
                return;
            }

            if (targetTickId > entityPredictor.EndTickId)
            {
                // We need to predict more
                log.Debug("target tickId is too far away {TargetTickId} {RollbackStackEndId}", targetTickId, entityPredictor.EndTickId);
                return;
            }

            if (targetTickId > entityPredictor.RollbackStack.PeekTickId())
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
            if (targetTickId > entityPredictor.EndTickId)
            {
                return;
            }
            
            if (targetTickId <= entityPredictor.TickId)
            {
                throw new ("can not predict because target is less than current, we should rollback instead");
            }

            var currentTickId = entityPredictor.RollbackStack.PeekTickId();

            while (currentTickId < targetTickId)
            {
                var input = entityPredictor.PredictedInputs.GetInputFromTickId(currentTickId);
                entityPredictor.Predict(input);
                currentTickId = currentTickId.Next;
            }
        }

        public static void Rollback(EntityPredictor entityPredictor, TickId targetTickId)
        {
            if (targetTickId > entityPredictor.EndTickId)
            {
                return;
            }

            if (targetTickId >= entityPredictor.TickId)
            {
                throw new Exception("can not go back in rollback because it is not less than current, we should predict instead");
            }

            var currentTickId = entityPredictor.RollbackStack.PeekTickId();

            while (currentTickId < targetTickId)
            {
                var undoPack = entityPredictor.RollbackStack.GetUndoPackFromTickId(currentTickId);
                RollBacker.RollBack(entityPredictor.AssignedAvatar, undoPack);
                currentTickId = currentTickId.Previous();
            }
        }
    }
}