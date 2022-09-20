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
    public sealed class InternetSimulatorOut : ITransportSend
    {
        readonly InternetSimulator internetSimulator;
        readonly IMonotonicTimeMs timeProvider;
        readonly ITransportSend wrappedTransport;

        public InternetSimulatorOut(ITransportSend wrappedTransport, IMonotonicTimeMs timeProvider,
            IRandom random, ILog log)
        {
            this.timeProvider = timeProvider;
            this.wrappedTransport = wrappedTransport;
            internetSimulator = new(timeProvider, random, log);
        }

        public Decision Decision => internetSimulator.Decision;
        public LatencySimulator LatencySimulator => internetSimulator.LatencySimulator;

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> octets)
        {
            internetSimulator.HandlePacket(endpointId, octets);
        }

        public void Update()
        {
            var now = timeProvider.TimeInMs;
            while (internetSimulator.PacketQueuePop.Dequeue(now, out var packet))
            {
                wrappedTransport.SendToEndpoint(packet.endPoint, packet.payload.Span);
            }
        }
    }
}