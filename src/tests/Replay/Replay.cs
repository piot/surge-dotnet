/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Replay.Serialization;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class ReplayTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public ReplayTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void WriteReplayWithGapFail()
    {
        var fileStream = FileStreamCreator.Create("replay.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));
        var replayRecorder =
            new ReplayWriter(new CompleteState(new(42), new byte[] { 0xca, 0xba }), versionInfo, fileStream);

        Assert.Throws<Exception>(() => replayRecorder.AddDeltaState(new(new(new(48), new(50)), new byte[] { 0xfe })));
    }

    [Fact]
    public void WriteReplayWithEarlierDeltaFail()
    {
        var fileStream = FileStreamCreator.Create("replay.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));

        var replayRecorder =
            new ReplayWriter(new CompleteState(new(42), new byte[] { 0xca, 0xba }), versionInfo, fileStream);

        Assert.Throws<Exception>(() => replayRecorder.AddDeltaState(new(new(new(40), new(42)), new byte[] { 0xfe })));
    }

    [Fact]
    public void WriteReplayWithDelta()
    {
        var fileStream = FileStreamCreator.Create("replay.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));

        var replayRecorder =
            new ReplayWriter(new CompleteState(new(42), new byte[] { 0xca, 0xba }), versionInfo, fileStream);

        replayRecorder.AddDeltaState(new(new(new(43), new(45)), new byte[] { 0xfe }));
    }

    [Fact]
    public void WriteAndReadReplay()
    {
        {
            using var fileStream = FileStreamCreator.Create("replay.temp");
            var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));

            var replayRecorder = new ReplayWriter(new CompleteState(new(42), new byte[] { 0xca, 0xba }), versionInfo,
                fileStream);
            replayRecorder.AddDeltaState(new(new(new(42), new(43)), new byte[] { 0xfe }));
            replayRecorder.Close();
        }

        {
            var fileStream = FileStreamCreator.OpenWithSeek("replay.temp");
            var replayPlayback = new ReplayReader(fileStream);
            Assert.Equal(42u, replayPlayback.FirstCompleteStateTickId.tickId);
            Assert.Equal(1, replayPlayback.ApplicationVersion.minor);
            Assert.Equal(2, replayPlayback.ApplicationVersion.patch);
            replayPlayback.ReadDeltaState();
            var completeState = replayPlayback.Seek(new(43));
            Assert.Equal(42u, completeState.TickId.tickId);
            var deltaState = replayPlayback.ReadDeltaState();
            Assert.Equal(43u, deltaState!.TickIdRange.Last.tickId);
        }
    }
}