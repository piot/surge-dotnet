/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.TransportReplay;
using Piot.Transport;
using Piot.Transport.Memory;
using Xunit.Abstractions;

namespace Tests.TransportReplay;

public sealed class TransportReplayTests
{
    private static readonly SemanticVersion applicationVersion = new(0, 2, 3);
    private readonly ILog log;

    public TransportReplayTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);

        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    private static ReadOnlySpan<byte> CreateReplayOctets()
    {
        var state = new MockState(0xF00D);
        var recordTarget = new OctetWriter(32 * 1024);

        var mockTransportReceive = new MemoryTransportReceive();

        var record = new TransportRecorder(mockTransportReceive, state, applicationVersion,
            new MonotonicTimeMockMs(new(23)),
            new(99), recordTarget);

        record.TickId = record.TickId.Next;
        mockTransportReceive.Feed(new(21), new byte[] { 0xca, 0xfe });

        var foundOctets = record.Receive(out var foundRemote);
        Assert.Equal(21, foundRemote.Value);
        Assert.Equal(new byte[] { 0xca, 0xfe }, foundOctets.ToArray());

        record.Close();

        return recordTarget.Octets;
    }

    [Fact]
    public void TestRecordAndPlayback()
    {
        var replayOctets = CreateReplayOctets();

        var playbackSource = new OctetReader(replayOctets);

        var state = new MockState(0);

        var mockReceiveTimeProvider = new MonotonicTimeMockMs(new(22));
        var playback = new TransportPlayback(state, applicationVersion, playbackSource, mockReceiveTimeProvider);
        Assert.Equal(0xf00dU, state.counter);

        var firstReadOctets = playback.Receive(out var firstEndpointId);
        Assert.Empty(firstReadOctets.ToArray());
        Assert.Equal(EndpointId.NoEndpoint, firstEndpointId);

        mockReceiveTimeProvider.TimeInMs = new(23);

        var readOctets = playback.Receive(out var readRemoteId);
        Assert.Equal(21, readRemoteId.Value);
        Assert.Equal(new byte[] { 0xca, 0xfe }, readOctets.ToArray());

        var nextReadOctets = playback.Receive(out var nextRemoteId);
        Assert.Empty(nextReadOctets.ToArray());
        Assert.Equal(EndpointId.NoEndpoint, nextRemoteId);
    }

    private class MockState : IOctetSerializable
    {
        public uint counter;

        public MockState(uint counter)
        {
            this.counter = counter;
        }

        public void Serialize(IOctetWriter writer)
        {
            writer.WriteUInt32(counter);
        }


        public void Deserialize(IOctetReader reader)
        {
            counter = reader.ReadUInt32();
        }
    }
}