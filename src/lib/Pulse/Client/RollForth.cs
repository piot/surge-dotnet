/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.ChangeMask;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Surge.LogicalInput;

namespace Piot.Surge.Pulse.Client
{
    public static class RollForth
    {
        public static void Rollforth(IEntity predictedEntity, LogicalInputQueue predictedInputs
            , RollbackQueue rollbackQueue, PredictionStateChecksumQueue predictionStateHistory
        )
        {
            predictedEntity.RollMode = EntityRollMode.Rollforth;

            foreach (var predictedInput in predictedInputs.Collection)
            {
                if (predictedEntity.GeneratedEntity is not IInputDeserialize inputDeserialize)
                {
                    throw new Exception("should be able to set input and rollforth to target entity");
                }

                var inputReader = new OctetReader(predictedInput.payload.Span);

                predictedEntity.Overwrite();
                inputDeserialize.SetInput(inputReader);
                predictedEntity.Tick();
                var changes = predictedEntity.GeneratedEntity.Changes();
                var undoWriter = new OctetWriter(1200);
                var changedFieldsMask = new ChangedFieldsMask(changes);

                ChangedFieldsMaskWriter.WriteChangedFieldsMask(undoWriter, changedFieldsMask);
                predictedEntity.SerializePrevious(changes, undoWriter);

                rollbackQueue.EnqueueUndoPack(predictedInput.appliedAtTickId, undoWriter.Octets);

                var savePredictedStateWriter = new OctetWriter(1200);
                predictedEntity.SerializeAll(savePredictedStateWriter);
                predictedEntity.SerializeCorrectionState(savePredictedStateWriter);
                predictionStateHistory.Enqueue(predictedInput.appliedAtTickId, savePredictedStateWriter.Octets);
            }
        }
    }
}