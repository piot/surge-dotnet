/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace Piot.Transport.Memory
{
    public struct Packet
    {
        public RemoteEndpointId remoteEndpointId;
        public ReadOnlyMemory<byte> payload;
    }

    public sealed class MemoryTransportReceive : ITransportReceive, ITransportEnqueue
    {
        private readonly Queue<Packet> packets = new();

        public void Feed(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            packets.Enqueue(new()
            {
                payload = payload.ToArray(),
                remoteEndpointId = remoteEndpointId
            });
        }

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            if (packets.Count <= 0)
            {
                remoteEndpointId = new();
                return ReadOnlySpan<byte>.Empty;
            }

            var packet = packets.Dequeue();

            remoteEndpointId = packet.remoteEndpointId;

            return packet.payload.Span;
        }
    }
}