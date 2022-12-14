/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Ecs2;
using Piot.Surge.Tick;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictStateSerializer
    {
        public static void SavePredictedState(EntityId predictedEntity, TickId tickIdAfterPrediction, IDataSender sender,
            ReadOnlySpan<byte> undoPack, ReadOnlySpan<byte> inputPack, PredictCollection predictCollection, bool isPredicting, ILog log)
        {
            var savePredictedStateWriter = new BitWriter(1200);
            var count = 0;
            foreach (var logicComponentTypeId in DataInfo.logicComponentTypeIds)
            {
                if (!sender.HasComponentTypeId(predictedEntity.Value, (ushort)logicComponentTypeId))
                {
                    continue;
                }
                ComponentTypeIdWriter.Write(savePredictedStateWriter, new((ushort)logicComponentTypeId));
                log.Debug("writing {EntityId} {ComponentTypeId}", predictedEntity, logicComponentTypeId);
                sender.WriteFull(savePredictedStateWriter, predictedEntity.Value, (ushort)logicComponentTypeId);
                count++;
            }

            ComponentTypeIdWriter.Write(savePredictedStateWriter, ComponentTypeId.None);

            if (count == 0 && isPredicting)
            {
                throw new Exception($"there were no state to save on the predicted {predictedEntity}");
            }

            log.Debug("Storing predict input for {TickId}", tickIdAfterPrediction);

            var predictedOctets = savePredictedStateWriter.Close(out _);
            predictCollection.EnqueuePredict(tickIdAfterPrediction, undoPack, inputPack,
                predictedOctets);
        }
    }
}