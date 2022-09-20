/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.TimeTick;
using Xunit.Abstractions;

namespace Tests.Pulse;

public sealed class TimeTickingTests
{
    readonly ILog log;

    public TimeTickingTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }


    [Fact]
    public void Test()
    {
        var calledCount = 0;
        var ticker = new TimeTicker(new(0), () => { calledCount++; }, new(16), log);

        for (var now = 0; now <= 420; now += 42)
        {
            ticker.Update(new(now));
        }

        Assert.Equal(26, calledCount);

        calledCount = 0;
        var ticker2 = new TimeTicker(new(0), () => { calledCount++; }, new(16), log);
        for (var now = 0; now <= 420; now += 1)
        {
            ticker2.Update(new(now));
        }

        Assert.Equal(26, calledCount);
    }
}