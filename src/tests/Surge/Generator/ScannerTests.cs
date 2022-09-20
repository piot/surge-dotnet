/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Generator;
using Xunit.Abstractions;

namespace Tests.Pulse.Generator;

public sealed class ScannerTests
{
    readonly ILog log;

    public ScannerTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    [Fact]
    public void FindLogics()
    {
        var allLogics = LogicScanner.ScanForLogics(log);
        Assert.Equal(2, allLogics.Count());

        var logicInfos = LogicInfoCollector.Collect(allLogics, log);
        Assert.Equal(2, logicInfos.Count());
    }
}