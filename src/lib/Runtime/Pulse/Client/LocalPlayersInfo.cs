/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Surge.Pulse.Client
{
    public readonly struct LocalPlayersInfo
    {
        public readonly Dictionary<byte, LocalPlayerInput> localPlayerInputs;

        public LocalPlayersInfo(Dictionary<byte, LocalPlayerInput> localPlayerInputs)
        {
            this.localPlayerInputs = localPlayerInputs;
        }
    }
}