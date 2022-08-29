/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge;
using Piot.Surge.ChangeMask;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaMasks;
using Xunit.Abstractions;

namespace Tests;

public class SnapshotDeltaEntityMasksMerger
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public SnapshotDeltaEntityMasksMerger(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    private static TickIdRange ToTickIdRange(ushort tickIdValue)
    {
        return new(new(tickIdValue), new(tickIdValue));
    }

    [Fact]
    public void TestMerge()
    {
        var firstMutable = new SnapshotDeltaEntityMasksMutable(ToTickIdRange(18));
        firstMutable.Deleted(new EntityId(5));
        firstMutable.SetChangedMask(new EntityId(1), 0x03);
        var first = new SnapshotDeltaEntityMasks(firstMutable);

        var secondMutable = new SnapshotDeltaEntityMasksMutable(ToTickIdRange(19));
        secondMutable.SetChangedMask(new EntityId(2), 0xf01);
        secondMutable.SetChangedMask(new EntityId(1), 0x80);
        secondMutable.SetChangedMask(new EntityId(3), 0x23);
        var second = new SnapshotDeltaEntityMasks(secondMutable);

        var thirdMutable = new SnapshotDeltaEntityMasksMutable(ToTickIdRange(20));
        thirdMutable.Deleted(new EntityId(2));
        thirdMutable.SetChangedMask(new EntityId(3), 0x01);
        var third = new SnapshotDeltaEntityMasks(thirdMutable);

        var merged = Piot.Surge.SnapshotDeltaMasks.SnapshotDeltaEntityMasksMerger.Merge(new[] { first, second, third });
        Assert.Equal(0x83u, merged.EntityMasks[1]);
        Assert.Equal(ChangedFieldsMask.DeletedMaskBit, merged.EntityMasks[2]);
        Assert.Equal(0x23u, merged.EntityMasks[3]);
        Assert.Equal(ChangedFieldsMask.DeletedMaskBit, merged.EntityMasks[5]);
        Assert.Equal(18u, merged.TickIdRange.startTickId.tickId);
        Assert.Equal(20u, merged.TickIdRange.lastTickId.tickId);
    }
}