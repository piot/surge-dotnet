/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Generator;
using Xunit.Abstractions;

namespace Tests;

public class GeneratorTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public GeneratorTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    [Fact]
    public void GenerateSourceCode()
    {
        var allLogics = LogicScanner.ScanForLogics(log);
        Assert.Equal(2, allLogics.Count());

        var logicInfos = LogicInfoCollector.Collect(allLogics, log);
        Assert.Equal(2, logicInfos.Count());

        var allInputs = GameInputScanner.ScanForGameInputs(log);
        var gameInputInfos = GameInputInfoCollector.Collect(allInputs, log);
        Assert.Equal(1, gameInputInfos.Count());

        var code = SourceGenerator.Generate(logicInfos, gameInputInfos.First());
        log.Info(code);

        const string target = "../../../../tests/ExampleGame/_Generated.cs";
        File.Delete(target);
        File.WriteAllText(target, code);
    }
}