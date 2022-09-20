/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.MonotonicTimeLowerBits;
using Xunit.Abstractions;

namespace Tests;

public sealed class MonotonicTimeLowerBitsTests
{
    readonly ILog log;

    public MonotonicTimeLowerBitsTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    static void TestLowerBits(ushort lowerBitsValue, ulong nowValue, ulong expectedResult)
    {
        var lowerBits = new MonotonicTimeLowerBits(lowerBitsValue);
        var now = new TimeMs((long)nowValue);
        var calculatedMonotonic = LowerBitsToMonotonic.LowerBitsToMonotonicMs(now, lowerBits);
        Assert.Equal((long)expectedResult, calculatedMonotonic.ms);
    }

    [Fact]
    public void TestCalculator()
    {
        TestLowerBits(0xfffe, 0x35ffff, 0x35fffe);
    }

    [Fact]
    public void TestCalculator2()
    {
        TestLowerBits(0xfffe, 0x360010, 0x35fffe);
    }

    [Fact]
    public void TestCalculator3()
    {
        TestLowerBits(0x0030, 0x360050, 0x360030);
    }

    [Fact]
    public void TestCalculator4()
    {
        TestLowerBits(0xff30, 0x360150, 0x35ff30);
    }

    [Fact]
    public void TestCalculatorException()
    {
        var lowerBits = new MonotonicTimeLowerBits(0xff30);
        var now = new TimeMs(0x36ff20);
        Assert.Throws<Exception>(() => LowerBitsToMonotonic.LowerBitsToMonotonicMs(now, lowerBits));
    }
}