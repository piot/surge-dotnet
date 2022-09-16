/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Xunit.Abstractions;

namespace Tests.Flood;

public sealed class ClogTests
{
    private readonly ILog log;

    public ClogTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestMatches()
    {
        var inputString = "This is a {Test} for {Something} good";
        var (_, argumentString) =
            ArgumentReplace.ReplaceArguments(inputString, new object[] { "TestThing", 42 });

        Assert.Equal("Test=TestThing, Something=42", argumentString);
    }
}