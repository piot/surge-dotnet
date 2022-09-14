/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Xunit.Abstractions;

namespace Tests.Flood;

public sealed class OctetWriterTests
{
    private readonly ILog log;

    public OctetWriterTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestShortRange()
    {
        var v = short.MinValue;
        var writer = new OctetWriter(16);

        writer.WriteInt16(v);
        writer.WriteInt16(-42);
        writer.WriteInt16(short.MaxValue);


        var reader = new OctetReader(writer.Octets);

        Assert.Equal(short.MinValue, reader.ReadInt16());
        Assert.Equal(-42, reader.ReadInt16());
        Assert.Equal(short.MaxValue, reader.ReadInt16());
    }
}