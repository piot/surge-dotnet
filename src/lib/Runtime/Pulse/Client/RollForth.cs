/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Ecs2;
using Piot.Surge.Entities;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.Pulse.Client
{
    public static class RollForth
    {
        public static void Rollforth(EntityId predictedEntity, PredictCollection predictCollection,
            IDataSender writeFromWorld, IDataReceiver clientReceiver, IEcsWorldSetter clientWorld, Action<EntityId> predictTickMethod, IOctetWriterWithResult undoScratchWriter, ILog log
        )
        {
            log.Info("Rollforth to end");

            clientWorld.Set(predictedEntity.Value, new RollMode
            {
                mode = EntityRollMode.Rollforth
            });

            foreach (var predictItem in predictCollection.RemainingItems())
            {
                var inputBitReader = new BitReader(predictItem.inputPackSetBeforeThisTick.Span, predictItem.inputPackSetBeforeThisTick.Span.Length * 8);

                while (true)
                {
                    var componentTypeId = ComponentTypeIdReader.Read(inputBitReader);
                    if (componentTypeId.id == ComponentTypeId.NoneValue)
                    {
                        break;
                    }

                    DataStreamReceiver.ReceiveNew(inputBitReader, predictedEntity.Value, componentTypeId.id, clientReceiver);
                }

                var sameInputAsPrevious =
                    new LogicalInput.LogicalInput(new(0), predictItem.tickId.Previous,
                        predictItem.inputPackSetBeforeThisTick.Span);
                log.Info("Set input and about to rollforth to {TickId}", predictItem.tickId);
                PredictAndSaver.PredictAndSave(predictedEntity, predictCollection, sameInputAsPrevious, writeFromWorld, undoScratchWriter,
                    predictTickMethod, true, log);
            }
        }
    }
}