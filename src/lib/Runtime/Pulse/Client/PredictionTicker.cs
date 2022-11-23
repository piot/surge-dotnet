/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionTicker
    {
        public static void Predict(EntityId predictedEntity, PredictMode predictMode, IOctetWriter undoWriter)
        {
            /* TODO:
            predictedEntity.CompleteEntity.RollMode = predictMode switch
            {
                PredictMode.RollingForth => EntityRollMode.Rollforth,
                PredictMode.Predicting => EntityRollMode.Predict
            };

            predictedEntity.CompleteEntity.ClearChanges();
            predictedEntity.CompleteEntity.Tick();
            predictedEntity.CompleteEntity.MovementSimulationTick();

            var changes = predictedEntity.CompleteEntity.Changes();
            

            predictedEntity.CompleteEntity.SerializePrevious(changes, undoWriter);
            */

        }
    }
}