/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.FieldMask;
using Xunit.Abstractions;

namespace Tests.Pulse.SnapshotEntityMasks;

public sealed class SnapshotDeltaEntityMaskMergeBits
{
    private readonly ILog log;

    public SnapshotDeltaEntityMaskMergeBits(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestMerge()
    {
        var shouldBeDeleted = MaskMerger.MergeBits(0x03, ChangedFieldsMask.DeletedMaskBit);
        Assert.Equal(ChangedFieldsMask.DeletedMaskBit, shouldBeDeleted);
    }

#if DEBUG
    [Fact]
#endif
    public void TestDeleteThenUpdatedFail()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            MaskMerger.MergeBits(ChangedFieldsMask.DeletedMaskBit, 0x03));
        Assert.Equal("first", exception.ParamName);
    }
}