/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;

namespace Piot.Surge.Pulse.Client
{
    public class LocalPlayerInput
    {
        public LogicalInputQueue PredictedInputs { get; } = new();

        public LocalPlayerIndex LocalPlayerIndex { get; }

        public LocalPlayerInput(LocalPlayerIndex localPlayerIndex)
        {
            LocalPlayerIndex = localPlayerIndex;
        }
    }
}