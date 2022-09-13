/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Hazy;

namespace Tests.Hazy;

public sealed class PacketQueueTests
{
    [Fact]
    public void TestPacketQueue()
    {
        var queue = new PacketQueue();

        var packet = new Packet
            { monotonicTimeMs = new(1030), payload = new byte[] { 0x80, 0x40 } };

        Assert.Equal(0, queue.Count);

        queue.AddPacket(packet);

        Assert.Equal(1, queue.Count);

        var nextPacket = new Packet
            { monotonicTimeMs = new(1020), payload = new byte[] { 0x40, 0x10 } };

        queue.AddPacket(nextPacket);

        Assert.Equal(2, queue.Count);

        var wasFound = queue.Dequeue(new(0), out var foundPacket);
        Assert.False(wasFound);
        Assert.Equal(0, foundPacket.monotonicTimeMs.ms);

        var nextWasFound = queue.Dequeue(new(1025), out var nextFoundPacket);
        Assert.True(nextWasFound);
        Assert.Equal(nextPacket.monotonicTimeMs.ms, nextFoundPacket.monotonicTimeMs.ms);
        Assert.Equal(0x040, nextFoundPacket.payload.ToArray()[0]);

        Assert.Equal(1, queue.Count);
    }
}