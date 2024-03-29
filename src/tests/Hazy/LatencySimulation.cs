/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Xunit.Abstractions;

namespace Tests.Hazy;

public sealed class LatencySimulationTests
{
    readonly ILog log;

    public LatencySimulationTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        log = new Log(logTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestLatency()
    {
        var random = new PseudoRandom(42);

        var latencySimulation = new LatencySimulator(20, 95, new(), random, log);

        for (var i = 0; i < 1000; i += 16)
        {
            var now = new TimeMs(i);
            latencySimulation.Update(now);
            log.DebugLowLevel("Latency {Latency}", latencySimulation.LatencyInMs);
            Assert.InRange(latencySimulation.LatencyInMs.ms, 20u, 95u);
        }
    }
}