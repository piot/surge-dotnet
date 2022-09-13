/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Stats;
using Xunit.Abstractions;

namespace Tests.Stats;

public sealed class StatsFormatTest
{
    private readonly ILog log;

    public StatsFormatTest(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        log = new Log(logTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestCountStat()
    {
        var s = BitFormatter.Format(1024);
        log.Debug("s={S}", s);

        Assert.Equal("1.0 kbit", s);
    }

    [Fact]
    public void TestCountStat2()
    {
        var s = BitFormatter.Format(8199);
        log.Debug("s={S}", s);

        Assert.Equal("8.2 kbit", s);
    }

    [Fact]
    public void TestCountStat3()
    {
        var s = BitFormatter.Format(8199 * 1000);
        log.Debug("s={S}", s);

        Assert.Equal("8.2 Mbit", s);
    }
}