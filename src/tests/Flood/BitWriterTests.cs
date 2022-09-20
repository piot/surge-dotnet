/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Xunit.Abstractions;

namespace Tests.Flood;

public sealed class BitWriterTests
{
    readonly ILog log;

    public BitWriterTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestShortRange()
    {
        var writer = new BitWriter(10);
    }
}