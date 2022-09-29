/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class FetchInputPackToLogicalInput
    {
        public static LogicalInput.LogicalInput[] FetchLogicalInputs(TickId predictTickId, IInputPackFetch inputPackFetch,
            IEnumerable<LocalPlayerIndex> localPlayerIndices, ILog log)
        {
            var logicalInputs = new List<LogicalInput.LogicalInput>();
            foreach (var localPlayerIndex in localPlayerIndices)
            {
                log.DebugLowLevel("Fetch input from {LocalPlayerIndex}", localPlayerIndex);
                var inputOctets = inputPackFetch.Fetch(localPlayerIndex);
                var logicalInput = new LogicalInput.LogicalInput(localPlayerIndex, predictTickId,
                    inputOctets);
                logicalInputs.Add(logicalInput);
            }

            return logicalInputs.ToArray();
        }
    }
}