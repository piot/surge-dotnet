/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionTicker
    {
        public static void Predict(IEntity predictedEntity, TickId tickIdBeforePredictTick, RollbackStack rollbackStack,
            PredictMode predictMode, IOctetWriterWithResult undoWriter)
        {
            predictedEntity.CompleteEntity.RollMode = predictMode switch
            {
                PredictMode.RollingForth => EntityRollMode.Rollforth,
                PredictMode.Predicting => EntityRollMode.Predict
            };

            predictedEntity.CompleteEntity.ClearChanges();
            predictedEntity.CompleteEntity.Tick();

            var changes = predictedEntity.CompleteEntity.Changes();


            predictedEntity.CompleteEntity.SerializePrevious(changes, undoWriter);

            Notifier.Notify(predictedEntity); // Notify also clear changes

            rollbackStack.PushUndoPack(tickIdBeforePredictTick, undoWriter.Octets);
        }
    }
}