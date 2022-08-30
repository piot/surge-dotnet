/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    /// <summary>
    ///     Represents a player that is sharing a connection. Usually when they are sharing the same device (e.g. split
    ///     screen).
    /// </summary>
    public class ConnectionPlayer
    {
        private RemoteEndpointId connectionId;
        private LocalPlayerIndex localPlayerIndex;

        public ConnectionPlayer(RemoteEndpointId connectionId, LocalPlayerIndex localPlayerIndex)
        {
            this.connectionId = connectionId;
            this.localPlayerIndex = localPlayerIndex;
        }

        public LogicalInputQueue LogicalInputQueue { get; } = new();
    }
}