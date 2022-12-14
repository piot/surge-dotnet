/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Ecs2;

namespace Piot.Surge.Pulse.Client
{
    public static class RollForth
    {
        public static void Rollforth(EntityId predictedEntity, PredictCollection predictCollection,
            IDataSender writeFromWorld, IOctetWriterWithResult undoScratchWriter, ILog log
        )
        {
            /* TODO:
            predictedEntity.CompleteEntity.RollMode = EntityRollMode.Rollforth;
        */
            log.Info("Rollforth to end");

            foreach (var predictedInput in predictCollection.RemainingItems())
            {
                /* TODO:
                 if (predictedEntity.CompleteEntity is not IInputDeserialize inputDeserialize)
                {
                    throw new("should be able to set input and rollforth to target entity");
                }

                var inputReader = new OctetReader(predictedInput.inputPackSetBeforeThisTick.Span);
                inputDeserialize.SetInput(inputReader);
                */

                var tempInput =
                    new LogicalInput.LogicalInput(new(0), predictedInput.tickId.Previous,
                        predictedInput.inputPackSetBeforeThisTick.Span);
                log.Info("Set input and about to rollforth to {TickId}", predictedInput.tickId);
                PredictAndSaver.PredictAndSave(predictedEntity, predictCollection, tempInput, writeFromWorld, undoScratchWriter,
                    PredictMode.RollingForth, true, log);
            }
        }
    }
}