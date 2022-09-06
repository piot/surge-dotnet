/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionTickAndStateSave
    {
        public static void PredictAndStateSave(IEntity predictedEntity, TickId tickIdBeforePredictTick,
            RollbackStack rollbackStack, PredictionStateChecksumQueue predictionStateHistory)
        {
            PredictionTicker.Predict(predictedEntity, tickIdBeforePredictTick, rollbackStack);
            var tickIdAfterPredict = tickIdBeforePredictTick.Next();
            PredictStateSerializer.SavePredictedState(predictedEntity, tickIdAfterPredict, predictionStateHistory);
        }
    }
}