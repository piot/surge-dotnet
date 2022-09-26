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
    public static class FetchAndStoreInput
    {
        public static void FetchAndStore(TickId predictTickId, IInputPackFetch inputPackFetch,
            IEnumerable<LocalPlayerInput> localPlayerInputs, ILog log)
        {
            foreach (var localPlayerInput in localPlayerInputs)
            {
                log.DebugLowLevel("Fetch input from {LocalPlayerIndex}", localPlayerInput.LocalPlayerIndex);
                var inputOctets = inputPackFetch.Fetch(localPlayerInput.LocalPlayerIndex);
                var logicalInput = new LogicalInput.LogicalInput(localPlayerInput.LocalPlayerIndex, predictTickId,
                    inputOctets);

                log.DebugLowLevel("Adding logical input {LogicalInput} for {LocalPredictor}", logicalInput,
                    localPlayerInput);
                localPlayerInput.AvatarPredictor.EntityPredictor.PredictedInputs.AddLogicalInput(logicalInput);
            }
        }
    }
}