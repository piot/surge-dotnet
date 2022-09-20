/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Stats;
using Xunit.Abstractions;

namespace Tests.Stats;

public sealed class StatsTests
{
    readonly ILog log;

    public StatsTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        log = new Log(logTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestCountStat()
    {
        var stat = new StatCountThreshold(4);

        Assert.Equal(0, stat.Stat.average);
        stat.Add(3);
        stat.Add(3);
        stat.Add(55);
        Assert.Equal(20, stat.Stat.average);
        stat.Add(3);
        Assert.Equal(16, stat.Stat.average);
        Assert.Equal(4u, stat.Stat.count);
        Assert.Equal(3, stat.Stat.min);
        Assert.Equal(55, stat.Stat.max);
        stat.Add(100);
        Assert.Equal(16, stat.Stat.average);
    }


    [Fact]
    public void TestTimeStat()
    {
        var now = new TimeMs(20);
        var minimumTime = new TimeMs(1000);

        var stat = new StatPerSecond(now, minimumTime);

        Assert.Equal(0, stat.Stat.average);
        stat.Add(-98);
        stat.Add(3);
        stat.Add(55);
        Assert.Equal(0, stat.Stat.average);
        stat.Add(3);

        var after = new TimeMs(now.ms + 1000);
        stat.Update(after);
        Assert.Equal(-98 + 3 + 55 + 3, stat.Stat.average);
        Assert.Equal(4u, stat.Stat.count);
        Assert.Equal(-37, stat.Stat.min);
        Assert.Equal(0, stat.Stat.max);
    }

    [Fact]
    public void TestTimeStat2()
    {
        var now = new TimeMs(20);
        var minimumTime = new TimeMs(1000);

        var stat = new StatPerSecond(now, minimumTime);

        Assert.Equal(0, stat.Stat.average);
        stat.Add(-3);
        stat.Add(3);
        stat.Add(55);
        Assert.Equal(0, stat.Stat.average);
        stat.Add(3);

        var firstUpdateNow = new TimeMs(now.ms + 500);
        stat.Update(firstUpdateNow);

        stat.Add(3);
        stat.Add(8);
        stat.Add(66);
        stat.Add(3);

        var secondUpdateNow = new TimeMs(now.ms + 1500);
        stat.Update(secondUpdateNow);

        Assert.Equal(80, stat.Stat.average);
        Assert.Equal(4u, stat.Stat.count);
        Assert.Equal(0, stat.Stat.min);
        Assert.Equal(116, stat.Stat.max);
    }
}