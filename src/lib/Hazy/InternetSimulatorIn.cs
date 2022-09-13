/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.Transport;

namespace Piot.Hazy
{
    public sealed class InternetSimulatorIn : ITransportReceive
    {
        private readonly InternetSimulator internetSimulator;
        private readonly IMonotonicTimeMs timeProvider;
        private readonly ITransportReceive wrappedTransport;

        public InternetSimulatorIn(ITransportReceive wrappedTransport, IMonotonicTimeMs timeProvider, IRandom random,
            ILog log)
        {
            this.wrappedTransport = wrappedTransport;
            internetSimulator = new InternetSimulator(timeProvider, random, log);
            this.timeProvider = timeProvider;
        }

        public Decision Decision => internetSimulator.Decision;
        public LatencySimulator LatencySimulator => internetSimulator.LatencySimulator;

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpoint)
        {
            var now = timeProvider.TimeInMs;
            var wasFound = internetSimulator.PacketQueuePop.Dequeue(now, out var packet);
            if (!wasFound)
            {
                remoteEndpoint = new RemoteEndpointId(0);
                return ReadOnlySpan<byte>.Empty;
            }

            remoteEndpoint = packet.endPoint;
            return packet.payload.Span;
        }

        public void Update()
        {
            for (var i = 0; i < 30; ++i)
            {
                var octets = wrappedTransport.Receive(out var endpoint);
                if (!octets.IsEmpty)
                {
                    internetSimulator.HandlePacket(endpoint, octets);
                }
            }
        }
    }
}