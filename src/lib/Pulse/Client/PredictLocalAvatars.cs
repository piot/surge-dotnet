/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictLocalAvatars
    {
        public static void PredictLocalPlayers(TickId predictTickId, IInputPackFetch inputPackFetch,
            IEnumerable<AvatarPredictor> localAvatarPredictors, bool usePrediction, ILog log)
        {
            foreach (var localAvatarPredictor in localAvatarPredictors)
            {
                var inputOctets = inputPackFetch.Fetch(localAvatarPredictor.LocalPlayerIndex);
                var logicalInput = new LogicalInput.LogicalInput
                {
                    appliedAtTickId = predictTickId,
                    payload = inputOctets.ToArray()
                };

                log.DebugLowLevel("Adding logical input {LogicalInput} for {LocalPredictor}", logicalInput,
                    localAvatarPredictor);
                localAvatarPredictor.PredictedInputs.AddLogicalInput(logicalInput);
                if (usePrediction)
                {
                    localAvatarPredictor.Predict(logicalInput);
                }
            }
        }
    }
}