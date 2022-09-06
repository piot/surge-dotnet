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
        public static void Predict(IEntity predictedEntity, TickId tickIdBeforePredictTick, RollbackStack rollbackStack)
        {
            predictedEntity.RollMode = EntityRollMode.Predict;
            predictedEntity.Overwrite();
            predictedEntity.Tick();

            var changes = predictedEntity.GeneratedEntity.Changes();

            var undoWriter = new OctetWriter(1200);

            predictedEntity.SerializePrevious(changes, undoWriter);

            Notifier.Notify(predictedEntity); // Notify also overwrites

            rollbackStack.PushUndoPack(tickIdBeforePredictTick, undoWriter.Octets);
        }
    }
}