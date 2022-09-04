/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Xunit.Abstractions;

namespace Tests.Flood;

public class FileStreams
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public FileStreams(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestFileWriteAndRead()
    {
        var testFilename = "testFile.bin";

        {
            using var writeStream = FileStreamCreator.Create(testFilename);
            writeStream.WriteUInt32(0xfefafbfc);
            writeStream.WriteUInt16(0xcafe);
        }

        {
            var readStream = FileStreamCreator.Open(testFilename);

            var encountered = readStream.ReadUInt32();
            Assert.Equal(0xfefafbfc, encountered);
        }

        {
            var readStreamWithSeek = FileStreamCreator.OpenWithSeek(testFilename);
            readStreamWithSeek.Seek(4);
            var readSecond = readStreamWithSeek.ReadUInt16();
            Assert.Equal(0xcafe, readSecond);
        }
    }
}