/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Hazy;
using Xunit.Abstractions;

namespace Tests.Hazy;

public class DecisionTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public DecisionTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestDecision()
    {
        var decision = new Decision(0.00001d, 0.002d, 0.01d, 0.001d);
        var random = new SystemRandom();
        var normalActionCount = 0;

        for (var i = 0; i < 100; i++)
        {
            var part = new PartsPerTenThousand((uint)random.Random((int)PartsPerTenThousand.Divisor));
            var action = decision.Decide(part);
            if (action == PacketAction.Normal)
            {
                normalActionCount++;
            }
        }

        Assert.InRange(normalActionCount, 90, 100);
    }
}