/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictStateSerializer
    {
        public static void SavePredictedState(IEntity predictedEntity, TickId tickIdAfterPrediction,
            PredictionStateChecksumQueue predictionStateHistory)
        {
            var savePredictedStateWriter = new OctetWriter(1200);
            predictedEntity.SerializeAll(savePredictedStateWriter);

            var savePhysicsStateWriter = new OctetWriter(1200);
            predictedEntity.SerializeCorrectionState(savePhysicsStateWriter);
            predictionStateHistory.Enqueue(tickIdAfterPrediction, savePredictedStateWriter.Octets,
                savePhysicsStateWriter.Octets);
        }
    }
}