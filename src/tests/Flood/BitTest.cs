/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Xunit.Abstractions;

namespace Tests.Flood;

public class TestBits
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public TestBits(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestWriteBits()
    {
        var writer = new BitWriter(1024);

        writer.WriteBits(0x431u, 12);

        var octets = writer.Close(out var bitLength);
        Assert.Equal(8, octets.Length);
        Assert.Equal(12, bitLength);
        Assert.Equal(0x43, octets[0]);
        Assert.Equal(0x10, octets[1]);
    }

    [Fact]
    public void TestReadBits()
    {
        var reader = new BitReader(new byte[] { 0x43, 0x10, 0, 0, 0, 0, 0, 0 }, 12);

        Assert.Equal(0x431u, reader.ReadBits(12));

        Assert.Throws<Exception>(() => reader.ReadBits(1));
    }


    [Fact]
    public void TestMultipleWriteBits()
    {
        var writer = new BitWriter(1024);
        var w = writer as IBitWriter;

        w.WriteBits(0x431088efu, 32);
        w.WriteBits(0x31Af34BBu, 30);
        w.WriteBits(0x3u, 5);

        var octets = writer.Close(out var bitLength);
        Assert.Equal(16, octets.Length);
        Assert.Equal(67, bitLength);
        Assert.Equal(0x43, octets[0]);
        Assert.Equal(0x10, octets[1]);
        Assert.Equal(0x88, octets[2]);
        Assert.Equal(0xef, octets[3]);
        Assert.Equal(0xc6, octets[4]);
        Assert.Equal(0x2f << 2, octets[5]);
        Assert.Equal((0x34 << 2) + 2, octets[6]);
        Assert.Equal(0x3b << 2, octets[7]);
        Assert.Equal(0x3 << 5, octets[8]);
        Assert.Equal(0, octets[9]);
    }

    [Fact]
    public void TestMultipleReadBits()
    {
        var writer = new BitWriter(1024);

        writer.WriteBits(0x431088efu, 32);
        writer.WriteBits(0x31Af34BBu, 30);
        writer.WriteBits(0x3u, 5);

        var octets = writer.Close(out var bitLength);
        Assert.Equal(67, bitLength);

        var reader = new BitReader(octets, bitLength);
        var r = reader as IBitReader;

        Assert.Equal(0x431088efu, r.ReadBits(32));
        Assert.Equal(0x31Af34BBu, r.ReadBits(30));
        Assert.Equal(0x3u, r.ReadBits(5));

        Assert.Throws<Exception>(() => r.ReadBits(1));
    }
}