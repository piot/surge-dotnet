/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictStateSerializer
    {
        public static void SavePredictedState(IEntity predictedEntity, TickId tickIdAfterPrediction,
            ReadOnlySpan<byte> undoPack, ReadOnlySpan<byte> inputPack, PredictCollection predictCollection)
        {
            var savePredictedStateWriter = new OctetWriter(1200);
            predictedEntity.CompleteEntity.SerializeAll(savePredictedStateWriter);

            var savePhysicsStateWriter = new OctetWriter(1200);
            predictedEntity.CompleteEntity.SerializeCorrectionState(savePhysicsStateWriter);
            predictCollection.EnqueuePredict(tickIdAfterPrediction, undoPack, inputPack,
                savePredictedStateWriter.Octets,
                savePhysicsStateWriter.Octets);
        }
    }
}