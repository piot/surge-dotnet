/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.ChangeMask;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Surge.Snapshot;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionTicker
    {
        public static void Predict(IEntity predictedEntity, TickId appliedAtTickId, RollbackQueue rollbackQueue,
            PredictionStateChecksumQueue predictionStateHistory)
        {
            predictedEntity.Overwrite();

            var nonAuthoritativeMode = new SimulationMode(TickMode.Predict);
            predictedEntity.Tick(nonAuthoritativeMode);

            var changes = predictedEntity.GeneratedEntity.Changes();
            var undoWriter = new OctetWriter(1200);
            var changedFieldsMask = new ChangedFieldsMask(changes);

            ChangedFieldsMaskWriter.WriteChangedFieldsMask(undoWriter, changedFieldsMask);
            predictedEntity.SerializePrevious(changes, undoWriter);

            rollbackQueue.EnqueueUndoPack(appliedAtTickId, undoWriter.Octets);

            var savePredictedStateWriter = new OctetWriter(1200);
            predictedEntity.SerializeAll(savePredictedStateWriter);
            predictedEntity.SerializeCorrectionState(savePredictedStateWriter);
            predictionStateHistory.Enqueue(appliedAtTickId, savePredictedStateWriter.Octets);
        }
    }
}