/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class InternetSimulatorOut : ITransportSend
    {
        private readonly ITransportSend baseTransport;
        readonly PacketQueue outQueue = new ();
        private readonly IMonotonicTimeMs timeProvider;
        private readonly RemoteEndpointId remoteEndpoint;
        private readonly Decision decider = new(3, 1, 5);
        private IRandom random;
        private readonly LatencySimulator latencySimulator;

        public InternetSimulatorOut(ITransportSend baseTransport, IMonotonicTimeMs timeProvider, 
            RemoteEndpointId remoteEndpoint, LatencySimulator latencySimulator, IRandom random)
        {
            this.random = random;
            this.baseTransport = baseTransport;
            this.timeProvider = timeProvider;
            this.remoteEndpoint = remoteEndpoint;
            this.latencySimulator = latencySimulator;
        }
        
        public void Update(Milliseconds now)
        {
            while (outQueue.Dequeue(now, out var packet))
            {
                baseTransport.SendToEndpoint(remoteEndpoint, packet.payload);
            }
        }

        public int LatencyInMs {
            get;
            set; 
        }

        private static byte[] RandomOctetArray(int octetSize, IRandom random)
        {
            var octets = new byte[octetSize];
            for (var i = 0; i < octetSize; ++i)
            {
                octets[i] = (byte) random.Random(255);
            }
            return octets;
        }
        
        public void SendToEndpoint(RemoteEndpointId endpointId, ReadOnlySpan<byte> octets)
        {
            var chance = random.Random(100);
            var packetAction = decider.Decide(new Percentage(chance));

            var now = timeProvider.TimeInMs;
            var withLatency = new Milliseconds(now.ms + latencySimulator.LatencyInMs.ms);

            switch (packetAction)
            {
                case PacketAction.Drop:
                    break;
                case PacketAction.Duplicate:
                    outQueue.AddPacket(new Packet {monotonicTimeMs = withLatency, payload = octets.ToArray()});
                    var nextLatency = new Milliseconds(now.ms + latencySimulator.LatencyInMs.ms+1);
                    outQueue.AddPacket(new Packet {monotonicTimeMs = nextLatency, payload = octets.ToArray()});
                    break;
                case PacketAction.Normal:
                    outQueue.AddPacket(new Packet {monotonicTimeMs = withLatency, payload = octets.ToArray()});
                    break;
                case PacketAction.Tamper:
                    var packetSize = random.Random(1100) + 10;
                    outQueue.AddPacket(new Packet {monotonicTimeMs = withLatency, payload = RandomOctetArray(packetSize, random)});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            };
        }
    }
}