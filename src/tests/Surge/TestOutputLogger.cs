/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Xunit.Abstractions;

namespace Tests;

public sealed class TestOutputLogger : ILogTarget
{
    readonly ITestOutputHelper output;

    public TestOutputLogger(ITestOutputHelper output)
    {
        this.output = output;
    }


    public void Log(LogLevel level, string prefix, string message, object[] args)
    {
        var strings = args.Select(static x => x.ToString());
        var values = args.Length > 0 ? $"({string.Join(", ", strings)})" : "";
        var line = $"{level,8} : [{prefix}] {message} {values}";

        output.WriteLine(line);
    }
}