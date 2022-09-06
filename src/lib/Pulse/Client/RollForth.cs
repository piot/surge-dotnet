/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.LogicalInput;

namespace Piot.Surge.Pulse.Client
{
    public static class RollForth
    {
        public static void Rollforth(IEntity predictedEntity, LogicalInputQueue predictedInputs
            , RollbackStack rollbackStack, PredictionStateChecksumQueue predictionStateHistory
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
                inputDeserialize.SetInput(inputReader);

                PredictionTickAndStateSave.PredictAndStateSave(predictedEntity, predictedInput.appliedAtTickId,
                    rollbackStack,
                    predictionStateHistory, PredictMode.RollingForth);
            }
        }
    }
}