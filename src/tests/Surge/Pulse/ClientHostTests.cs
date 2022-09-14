/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.Surge;
using Piot.Surge.Compress;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Transport;
using Piot.Transport.Memory;
using Tests.ExampleGame;
using Tests.Surge.ExampleGame;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class FetchInput
{
    public GameInput ReadFromDevice(LocalPlayerIndex localPlayerIndex)
    {
        return new GameInput
        {
            primaryAbility = true,
            secondaryAbility = false
        };
    }
}

public sealed class ClientHostTests
{
    private readonly ILog log;
    private readonly FetchInput mockInput = new();

    public ClientHostTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    private Client CreateClient(Milliseconds now, ITransport transport)
    {
        var clientDeltaTime = new Milliseconds(16);
        var inputFetch = new GeneratedInputPackFetch();
        inputFetch.GameSpecificInputFetch = mockInput.ReadFromDevice;
        var notifyWorld = new GeneratedNotifyEntityCreation();
        var generatedEvent = new GeneratedEventProcessor(new ShortEvents(log.SubLog("ShortEvents")));
        var entityContainerWithGhostCreator =
            new WorldWithGhostCreator(new GeneratedEntityGhostCreator(), notifyWorld, false);

        var client = new Client(log.SubLog("Client"), now, clientDeltaTime, entityContainerWithGhostCreator,
            generatedEvent, transport,
            DefaultMultiCompressor.Create(), inputFetch);

        return client;
    }

    private Host CreateHost(Milliseconds now, ITransport transport)
    {
        var worldWithDetectChanges = new AuthoritativeWorld();
        var host = new Host(transport, DefaultMultiCompressor.Create(), DefaultMultiCompressor.DeflateCompressionIndex,
            worldWithDetectChanges, now, log.SubLog("Host"));
        return host;
    }

    [Fact]
    public void TestClientAndHostUpdates()
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

        var client = CreateClient(initNow, clientTransport);
        var host = CreateHost(initNow, hostTransportToUse);

        //var world = host.AuthoritativeWorld;
        //var spawnedEntity = world.SpawnEntity(new AvatarLogicEntityInternal());
        //log.Info("Spawned entity {Entity}", spawnedEntity);

        for (var iteration = 0; iteration < 62; iteration++)
        {
            var now = new Milliseconds(20 + iteration * 14);
            timeProvider.TimeInMs = now;
            internetSimulatedHostTransport?.Update();
            client.Update(now);
            host.Update(now);
        }
    }
}