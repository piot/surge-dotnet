/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Xunit.Abstractions;

namespace Tests.Hazy;

public class SystemRandom : IRandom
{
    private readonly Random rand = new();

    public int Random(int max)
    {
        return rand.Next(max);
    }
}

public class LatencySimulationTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public LatencySimulationTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    [Fact]
    public void TestLatency()
    {
        var random = new SystemRandom();

        var latencySimulation = new LatencySimulator(20, 95, new Milliseconds(), random, log);

        for (var i = 0; i < 1000; i += 16)
        {
            var now = new Milliseconds(i);
            latencySimulation.Update(now);
            Assert.InRange(latencySimulation.LatencyInMs.ms, 20, 95);
        }
    }
}