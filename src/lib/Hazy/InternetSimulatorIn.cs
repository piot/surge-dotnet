/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class InternetSimulatorIn : ITransportReceive
    {
        private readonly ITransportReceive baseTransport;
        readonly PacketQueue inQueue = new();
        private readonly IMonotonicTimeMs timeProvider;

        public InternetSimulatorIn(ITransportReceive baseTransport, IMonotonicTimeMs timeProvider)
        {
            this.baseTransport = baseTransport;
            this.timeProvider = timeProvider;
        }

        public void Update(Milliseconds now)
        {
            for (var i = 0; i < 30; ++i)
            {
                var octets = baseTransport.Receive(out var endpoint);
                inQueue.AddPacket(new Packet
                    { payload = octets.ToArray(), endPoint = endpoint, monotonicTimeMs = now });
            }
        }

        public int LatencyInMs { get; set; }

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpoint)
        {
            if (inQueue.Count == 0)
            {
                remoteEndpoint = new RemoteEndpointId(0);
                return new ReadOnlySpan<byte>();
            }

            var now = timeProvider.TimeInMs;
            
            var wasFound = inQueue.Dequeue(now, out var packet);
            if (wasFound)
            {
                remoteEndpoint = packet.endPoint;
                return packet.payload;
            }
            
            remoteEndpoint = new RemoteEndpointId( 0);
            return new ReadOnlySpan<byte>();
        }
    }
}