/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.ChangeMask;
using Xunit.Abstractions;

namespace Tests;

public class SnapshotDeltaEntityMaskMergeBits
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public SnapshotDeltaEntityMaskMergeBits(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestMerge()
    {
        var shouldBeDeleted = ChangedFieldsMerger.MergeBits(0x03, ChangedFieldsMask.DeletedMaskBit);
        Assert.Equal(ChangedFieldsMask.DeletedMaskBit, shouldBeDeleted);
    }

#if DEBUG
    [Fact]
#endif
    public void TestDeleteThenUpdatedFail()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ChangedFieldsMerger.MergeBits(ChangedFieldsMask.DeletedMaskBit, 0x03));
        Assert.Equal("first", exception.ParamName);
    }
}