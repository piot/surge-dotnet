/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictAndSaver
    {
        public static void PredictAndSave(EntityId assignedAvatar, PredictCollection predictCollection,
            LogicalInput.LogicalInput logicalInputSetBeforePrediction, IOctetWriterWithResult undoWriter,
            PredictMode predictMode, bool doActualPrediction)
        {
            var tickIdAfterPredictTick = logicalInputSetBeforePrediction.appliedAtTickId.Next;
            if (doActualPrediction)
            {
                PredictionTicker.Predict(assignedAvatar, predictMode,
                    undoWriter);
            }

            PredictStateSerializer.SavePredictedState(assignedAvatar, tickIdAfterPredictTick, undoWriter.Octets,
                logicalInputSetBeforePrediction.payload.Span, predictCollection);
        }
    }
}