/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class InternetSimulatorOut : ITransportSend
    {
        private readonly Decision decider = new(3, 1, 5, 1);
        private readonly LatencySimulator latencySimulator;
        private readonly PacketQueue outQueue = new();
        private readonly IRandom random;
        private readonly IMonotonicTimeMs timeProvider;
        private readonly ITransportSend wrappedTransport;

        public InternetSimulatorOut(ITransportSend wrappedTransport, IMonotonicTimeMs timeProvider,
            IRandom random, ILog log)
        {
            this.random = random;
            this.wrappedTransport = wrappedTransport;
            this.timeProvider = timeProvider;
            latencySimulator =
                new LatencySimulator(30, 220, timeProvider.TimeInMs, random, log.SubLog("InternetSimLatency"));
        }

        public Milliseconds LatencyInMs => latencySimulator.LatencyInMs;

        public void SendToEndpoint(RemoteEndpointId endpointId, ReadOnlySpan<byte> octets)
        {
            var chance = (uint)random.Random((int)PartsPerTenThousand.Divisor);
            var packetAction = decider.Decide(new PartsPerTenThousand(chance));

            var now = timeProvider.TimeInMs;
            var withLatency = new Milliseconds(now.ms + latencySimulator.LatencyInMs.ms);

            switch (packetAction)
            {
                case PacketAction.Drop:
                    break;
                case PacketAction.Reorder:
                {
                    var insertTime = withLatency.ms - 5;
                    var wasFound = outQueue.FindFirstPacketForEndpoint(endpointId, out var foundPacket);
                    if (wasFound)
                    {
                        insertTime = foundPacket.monotonicTimeMs.ms - 5;
                    }

                    outQueue.AddPacket(new Packet
                        { monotonicTimeMs = new Milliseconds(insertTime), payload = octets.ToArray() });
                }
                    break;
                case PacketAction.Duplicate:
                    outQueue.AddPacket(new Packet { monotonicTimeMs = withLatency, payload = octets.ToArray() });
                    var nextLatency = new Milliseconds(now.ms + latencySimulator.LatencyInMs.ms + 1);
                    outQueue.AddPacket(new Packet { monotonicTimeMs = nextLatency, payload = octets.ToArray() });
                    break;
                case PacketAction.Normal:
                    outQueue.AddPacket(new Packet { monotonicTimeMs = withLatency, payload = octets.ToArray() });
                    break;
                case PacketAction.Tamper:
                {
                    var packetSize = random.Random(1100) + 10;
                    outQueue.AddPacket(new Packet
                        { monotonicTimeMs = withLatency, payload = RandomOctetArray(packetSize, random) });
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Update(Milliseconds now)
        {
            while (outQueue.Dequeue(now, out var packet))
            {
                wrappedTransport.SendToEndpoint(packet.endPoint, packet.payload);
            }
        }

        private static byte[] RandomOctetArray(int octetSize, IRandom random)
        {
            var octets = new byte[octetSize];
            for (var i = 0; i < octetSize; ++i)
            {
                octets[i] = (byte)random.Random(255);
            }

            return octets;
        }
    }
}