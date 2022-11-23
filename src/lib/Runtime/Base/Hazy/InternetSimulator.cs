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
    public sealed class InternetSimulator
    {
        readonly PacketQueue packetQueue = new();
        readonly IRandom random;
        readonly IMonotonicTimeMs timeProvider;

        public InternetSimulator(IMonotonicTimeMs timeProvider,
            IRandom random, ILog log)
        {
            this.random = random;
            this.timeProvider = timeProvider;
            LatencySimulator =
                new(20, 125, timeProvider.TimeInMs, random, log.SubLog("InternetSimLatency"));
        }

        public LatencySimulator LatencySimulator { get; }

        public Decision Decision { get; } = new(0.00002d, 0.002d, 0.01d, 0.001d);

        public IPacketQueuePop PacketQueuePop => packetQueue;

        public void Update(TimeMs now)
        {
            LatencySimulator.Update(now);
        }

        public void HandlePacket(EndpointId endpointId, ReadOnlySpan<byte> octets)
        {
            var chance = (uint)random.Random((int)PartsPerTenThousand.Divisor);
            var packetAction = Decision.Decide(new(chance));

            var now = timeProvider.TimeInMs;
            var withLatency = new TimeMs(now.ms + LatencySimulator.LatencyInMs.ms);

            switch (packetAction)
            {
                case PacketAction.Drop:
                    break;
                case PacketAction.Reorder:
                {
                    var insertTime = withLatency.ms - 5;
                    var wasFound = packetQueue.FindFirstPacketForEndpoint(endpointId, out var foundPacket);
                    if (wasFound)
                    {
                        insertTime = foundPacket.monotonicTimeMs.ms - 5;
                    }

                    packetQueue.AddPacket(new(new(insertTime), endpointId, octets));
                }
                    break;
                case PacketAction.Duplicate:
                    packetQueue.AddPacket(new(withLatency, endpointId, octets));
                    var nextLatency = new TimeMs(now.ms + LatencySimulator.LatencyInMs.ms + 1);
                    packetQueue.AddPacket(new(nextLatency, endpointId, octets));
                    break;
                case PacketAction.Normal:
                    packetQueue.AddPacket(new(withLatency, endpointId, octets));
                    break;
                case PacketAction.Tamper:
                {
                    var packetSize = random.Random(1100) + 10;
                    packetQueue.AddPacket(new(withLatency, endpointId, RandomOctetArray(packetSize, random)));
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static byte[] RandomOctetArray(int octetSize, IRandom random)
        {
            var octets = new byte[octetSize];
            for (var i = 0; i < octetSize; ++i)
            {
                octets[i] = (byte)random.Random(256);
            }

            return octets;
        }
    }
}