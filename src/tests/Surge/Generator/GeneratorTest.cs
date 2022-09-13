/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Generator;
using Xunit.Abstractions;

namespace Tests.Generator;

public sealed class GeneratorTests
{
    private readonly ILog log;

    public GeneratorTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget);
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
        Assert.Single(gameInputInfos);

        var inputFetchers = InputFetchScanner.ScanForInputFetchMethods(log);
        Assert.Single(inputFetchers);
        var inputFetchInfos = GameInputFetchInfoCollector.Collect(inputFetchers, log);

        var shortLivedEvents = ShortLivedEventsScanner.ScanForEventInterfaces(log);
        Assert.Single(shortLivedEvents);

        var shortLivedEventsMethods = ShortLivedEventsCollector.Collect(shortLivedEvents.First());

        var code = SourceGenerator.Generate(logicInfos, gameInputInfos.First(), inputFetchInfos.First(),
            shortLivedEventsMethods);

        const string target = "../../../../tests/Surge/ExampleGame/_Generated.cs";
        File.Delete(target);
        File.WriteAllText(target, code);
    }
}