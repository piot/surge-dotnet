/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.TimeTicker;
using Xunit.Abstractions;

namespace Tests;

public class TimeTickerTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public TimeTickerTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    [Fact]
    public void TickZeroTimes()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(32);
        var ticker = new TimeTicker(new Milliseconds(10), () => { tickCount++; }, deltaTimeMs, log);

        ticker.Update(new Milliseconds(10));

        Assert.Equal(0, tickCount);
    }

    [Fact]
    public void TickOneTime()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(32);
        var now = new Milliseconds(10);

        var ticker = new TimeTicker(now, () => { tickCount++; }, deltaTimeMs, log);

        ticker.Update(new Milliseconds(10 + 32 + 31));

        Assert.Equal(1, tickCount);
    }

    [Fact]
    public void TickTwoTimes()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(32);
        var now = new Milliseconds(10);

        var ticker = new TimeTicker(now, () => { tickCount++; }, deltaTimeMs, log.SubLog("TwoTimes"));

        ticker.Update(new Milliseconds(10 + 32 + 32));

        Assert.Equal(2, tickCount);
    }

    [Fact]
    public void IllegalDeltaTime()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(0);
        var now = new Milliseconds(42);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new TimeTicker(now, () => { tickCount++; }, deltaTimeMs, log.SubLog("IllegalDeltaTime"));
        });

        log.Debug("exception {Exception}", exception);

        Assert.Equal("deltaTimeMs", exception.ParamName);
        Assert.Equal(0, tickCount);
    }


#if DEBUG
    [Fact]
#endif
    public void IllegalUpdateTime()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(32);
        var now = new Milliseconds(10);

        var ticker = new TimeTicker(now, () => { tickCount++; }, deltaTimeMs, log.SubLog("IllegalUpdateTime"));

        ticker.Update(new Milliseconds(10 + 32 + 31));
        Assert.Equal(1, tickCount);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            ticker.Update(new Milliseconds(10 + 32 + 30));
        });

        Assert.Equal("now", exception.ParamName);
        Assert.Equal(1, tickCount);
    }


    [Fact]
    public void CheckThatRestIsUsed()
    {
        var tickCount = 0;
        var deltaTimeMs = new Milliseconds(32);
        var now = new Milliseconds(10);

        var ticker = new TimeTicker(now, () => { tickCount++; }, deltaTimeMs, log.SubLog("CheckThatRestIsUsed"));

        ticker.Update(new Milliseconds(10 + 32 + 31));
        Assert.Equal(1, tickCount);

        ticker.Update(new Milliseconds(10 + 32 + 33));
        Assert.Equal(2, tickCount);
    }
}