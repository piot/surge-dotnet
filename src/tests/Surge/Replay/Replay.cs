/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Raff;
using Piot.SerializableVersion;
using Piot.Surge.Replay.Serialization;
using Xunit.Abstractions;

namespace Tests.Surge.Replay;

public static class Constants
{
    public static FourCC ReplayName = FourCC.Make("qps1");
    public static FourCC ReplayIcon = new(0xF09F8E9E); // Film frames

    public static FourCC CompleteStateName = FourCC.Make("qst1");
    public static FourCC CompleteStateIcon = new(0xF09F96BC); // Picture Frame

    public static FourCC DeltaStateName = FourCC.Make("qds1");
    public static FourCC DeltaStateIcon = new(0xF09FA096); // Right Arrow


    public static ReplayFileSerializationInfo ReplayInfo = new(
        new(ReplayIcon, ReplayName),
        new(CompleteStateIcon, CompleteStateName),
        new(DeltaStateIcon, DeltaStateName)
    );
}

public sealed class ReplayTests
{
    readonly ILog log;

    public ReplayTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }


    [Fact]
    public void WriteReplayWithGapFail()
    {
        var fileStream = FileStreamCreator.Create("write_replay_with_gap_fail.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));
        var replayRecorder =
            new ReplayWriter(new(new(49200), new(42), new byte[] { 0xca, 0xba }), versionInfo,
                Constants.ReplayInfo, fileStream);

        // TODO: move to other tests
        /*
        Assert.Throws<Exception>(() =>
            replayRecorder.AddDeltaState(new(new(42439), new(new(48), new(50)), new byte[] { 0xfe })));
            */
    }

    [Fact]
    public void WriteReplayWithEarlierDeltaFail()
    {
        var fileStream = FileStreamCreator.Create("write_replay_with_earlier_delta_fail.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));

        var replayRecorder =
            new ReplayWriter(new(new(49200), new(42), new byte[] { 0xca, 0xba }), versionInfo,
                Constants.ReplayInfo, fileStream);

        // TODO: Move to snapshot tests
        /*
        Assert.Throws<Exception>(() =>
            replayRecorder.AddDeltaState(new(new(4949), new(new(40), new(42)), new byte[] { 0xfe })));
            */
    }

    [Fact]
    public void WriteReplayWithDelta()
    {
        var fileStream = FileStreamCreator.Create("write_replay_with_delta.temp");
        var versionInfo = new ReplayVersionInfo(new(0, 1, 2), new(3, 4, 5));

        var replayRecorder =
            new ReplayWriter(new(new(49200), new(42), new byte[] { 0xca, 0xba }), versionInfo,
                Constants.ReplayInfo, fileStream);

        replayRecorder.AddDeltaState(new(new(10459), new(new(43), new(45)), new byte[] { 0xfe }));
    }

    [Fact]
    public void WriteAndReadReplay()
    {
        const string filename = "write_and_read_replay.temp";
        var applicationVersion = new SemanticVersion(0, 1, 2);

        {
            using var fileStream = FileStreamCreator.Create(filename);
            var versionInfo = new ReplayVersionInfo(applicationVersion, new(3, 4, 5));

            var replayRecorder = new ReplayWriter(new(new(49200), new(42), new byte[] { 0xca, 0xba }),
                versionInfo,
                Constants.ReplayInfo, fileStream);
            replayRecorder.AddDeltaState(new(new(49200), new(new(42), new(43)), new byte[] { 0xfe }));
            replayRecorder.Close();
        }

        {
            var fileStream = FileStreamCreator.OpenWithSeek(filename);
            var replayPlayback = new ReplayReader(applicationVersion, Constants.ReplayInfo, fileStream);
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