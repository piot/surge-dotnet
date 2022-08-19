using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class InternetSimulator
    {
        private readonly PacketQueue packetQueue = new();
        private readonly IRandom random;
        private readonly IMonotonicTimeMs timeProvider;

        public InternetSimulator(IMonotonicTimeMs timeProvider,
            IRandom random, ILog log)
        {
            this.random = random;
            this.timeProvider = timeProvider;
            LatencySimulator =
                new LatencySimulator(20, 125, timeProvider.TimeInMs, random, log.SubLog("InternetSimLatency"));
        }

        public LatencySimulator LatencySimulator { get; }

        public Decision Decision { get; } = new(0.00002d, 0.002d, 0.01d, 0.001d);

        public IPacketQueuePop PacketQueuePop => packetQueue;

        public void HandlePacket(RemoteEndpointId endpointId, ReadOnlySpan<byte> octets)
        {
            var chance = (uint)random.Random((int)PartsPerTenThousand.Divisor);
            var packetAction = Decision.Decide(new PartsPerTenThousand(chance));

            var now = timeProvider.TimeInMs;
            var withLatency = new Milliseconds(now.ms + LatencySimulator.LatencyInMs.ms);

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

                    packetQueue.AddPacket(new Packet
                        { monotonicTimeMs = new Milliseconds(insertTime), payload = octets.ToArray() });
                }
                    break;
                case PacketAction.Duplicate:
                    packetQueue.AddPacket(new Packet { monotonicTimeMs = withLatency, payload = octets.ToArray() });
                    var nextLatency = new Milliseconds(now.ms + LatencySimulator.LatencyInMs.ms + 1);
                    packetQueue.AddPacket(new Packet { monotonicTimeMs = nextLatency, payload = octets.ToArray() });
                    break;
                case PacketAction.Normal:
                    packetQueue.AddPacket(new Packet { monotonicTimeMs = withLatency, payload = octets.ToArray() });
                    break;
                case PacketAction.Tamper:
                {
                    var packetSize = random.Random(1100) + 10;
                    packetQueue.AddPacket(new Packet
                        { monotonicTimeMs = withLatency, payload = RandomOctetArray(packetSize, random) });
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static byte[] RandomOctetArray(int octetSize, IRandom random)
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