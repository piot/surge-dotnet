/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Ecs2;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictAndSaver
    {
        public static void PredictAndSave(EntityId assignedAvatar, PredictCollection predictCollection,
            LogicalInput.LogicalInput logicalInputSetBeforePrediction, IDataSender saveFromWorld, IOctetWriterWithResult undoWriter, Action<EntityId> predictTickMethod, bool doActualPrediction, ILog log)
        {
            var tickIdAfterPredictTick = logicalInputSetBeforePrediction.appliedAtTickId.Next;
            if (doActualPrediction)
            {
                predictTickMethod(assignedAvatar);
            }

            if (logicalInputSetBeforePrediction.payload.IsEmpty)
            {
                throw new Exception("we should provide some input");
            }

            PredictStateSerializer.SavePredictedState(assignedAvatar, tickIdAfterPredictTick, saveFromWorld, undoWriter.Octets,
                logicalInputSetBeforePrediction.payload.Span, predictCollection, doActualPrediction, log);
        }
    }
}