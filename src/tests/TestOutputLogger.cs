/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Xunit.Abstractions;

namespace Piot.Clog;

public class TestOutputLogger : ILogTarget
{
    private readonly ITestOutputHelper output;

    public TestOutputLogger(ITestOutputHelper output)
    {
        this.output = output;
    }

    public void Log(LogLevel level, string message, object[] args)
    {
        var strings = args.Select(x => x.ToString());
        var allStrings = level + ": " + message + " " + string.Join(",", strings);
        output.WriteLine(allStrings);
    }
}