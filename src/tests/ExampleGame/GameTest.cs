/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.Surge.MemoryTransport;
using Tests.ExampleGame;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class GameTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public GameTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void TestExampleGame()
    {
        var initNow = new Milliseconds(10);

        var (clientTransport, hostTransport) = MemoryTransportFactory.CreateClientAndHostTransport();

        const bool useInternetSimulation = true;

        var hostTransportToUse = hostTransport;
        InternetSimulatorTransport? internetSimulatedHostTransport;

        var timeProvider = new MonotonicTimeMockMs(initNow);
        if (useInternetSimulation)
        {
            var randomizer = new PseudoRandom(0x48019422);
            internetSimulatedHostTransport =
                new InternetSimulatorTransport(hostTransport, timeProvider, randomizer,
                    log.SubLog("InternetSimulator"));
            hostTransportToUse = internetSimulatedHostTransport;
        }


        var clientGame = new Game(clientTransport, log.SubLog("client"));
        var hostGame = new Game(hostTransport, log.SubLog("host"));


        //var world = host.AuthoritativeWorld;
        //var spawnedEntity = world.SpawnEntity(new AvatarLogicEntityInternal());
        //log.Info("Spawned entity {Entity}", spawnedEntity);

        for (var iteration = 0; iteration < 62; iteration++)
        {
            var now = new Milliseconds(20 + iteration * 14);
            timeProvider.TimeInMs = now;
            internetSimulatedHostTransport?.Update();
            clientGame.Update(now);
            hostGame.Update(now);
        }
    }
}