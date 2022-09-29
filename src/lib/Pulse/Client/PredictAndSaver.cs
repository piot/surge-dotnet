/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictAndSaver
    {
        public static void PredictAndSave(IEntity assignedAvatar, PredictCollection predictCollection, LogicalInput.LogicalInput logicalInput, IOctetWriterWithResult undoWriter, PredictMode predictMode, bool doActualPrediction)
        {
            if (doActualPrediction)
            {
                PredictionTicker.Predict(assignedAvatar, predictMode,
                    undoWriter);
            }

            var tickIdAfterPredict = logicalInput.appliedAtTickId.Next;
            PredictStateSerializer.SavePredictedState(assignedAvatar, tickIdAfterPredict, undoWriter.Octets, logicalInput.payload.Span, predictCollection);
        }
    }
}