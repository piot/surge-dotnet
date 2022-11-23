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
        public EndpointId endpointId;
        public ReadOnlyMemory<byte> payload;
    }

    public sealed class MemoryTransportReceive : ITransportReceive, ITransportEnqueue
    {
        readonly Queue<Packet> packets = new();

        public void Feed(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            packets.Enqueue(new()
            {
                payload = payload.ToArray(), endpointId = endpointId
            });
        }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            if (packets.Count <= 0)
            {
                endpointId = new();
                return ReadOnlySpan<byte>.Empty;
            }

            var packet = packets.Dequeue();

            endpointId = packet.endpointId;

            return packet.payload.Span;
        }
    }
}