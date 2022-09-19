/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    /// <summary>
    ///     Represents a player that is sharing a connection. Usually when they are sharing the same device (e.g. split
    ///     screen).
    /// </summary>
    public sealed class ConnectionPlayer
    {
        private readonly EndpointId connectionId;
        public IEntity? AssignedPredictEntity;

        public ConnectionPlayer(EndpointId connectionId, LocalPlayerIndex localPlayerIndex)
        {
            this.connectionId = connectionId;
            LocalPlayerIndex = localPlayerIndex;
        }

        public LogicalInputQueue LogicalInputQueue { get; } = new();

        public LocalPlayerIndex LocalPlayerIndex { get; }

        public override string ToString()
        {
            return
                $"[ConnectionPlayer connectionId:{connectionId} localPlayerIndex:{LocalPlayerIndex} inputQueue:{LogicalInputQueue.Count}]";
        }
    }
}