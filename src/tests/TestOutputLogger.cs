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

    public void Log(LogLevel level, string prefix, string message, object[] args)
    {
        var strings = args.Select(x => x.ToString());
        var values = args.Length > 0 ? $"({string.Join(", ", strings)})" : "";
        var line = $"{level,8} : [{prefix}] {message} {values}";
        output.WriteLine(line);
    }
}