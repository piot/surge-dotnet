/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Types;
using Piot.Transport;
using Piot.Transport.Memory;
using Xunit.Abstractions;

namespace Tests.ExampleGame;

public sealed class GameSync
{
    private readonly ILog log;

    public GameSync(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);

        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void ExampleGameSync()
    {
        var initNow = new Milliseconds(10);

        var (clientTransport, hostTransport) = MemoryTransportFactory.CreateClientAndHostTransport();

        var multiCompressor = DefaultMultiCompressor.Create();
        var timeProvider = new MonotonicTimeMockMs(initNow);

        var clientGame = new Game(clientTransport, multiCompressor, false, log.SubLog("GameClient"));
        var hostGame = new Game(hostTransport, multiCompressor, true, log.SubLog("GameHost"));


        var enqueue = new GeneratedEventEnqueue(hostGame.Host!.ShortLivedEventStream);


        var mockInput = new MockInputFetch(log.SubLog("MockInput"));

        clientGame.Client!.InputFetch = mockInput;

        var entitySpawner = hostGame.GeneratedHostEntitySpawner;
        var (spawnedEntity, spawnedHostAvatar) = entitySpawner.SpawnAvatarLogic(new AvatarLogic
        {
            ammoCount = 10,
            manaAmount = 16
        });
        log.Info("Spawned entity {Entity}", spawnedHostAvatar);

        var spawnedCalled = 0;
        var fireButtonChanged = 0;
        var fireballFireCount = 0;
        var ammoCountChanged = 0;
        var chainLightningCount = 0;

        clientGame.GeneratedNotifyEntityCreation.OnSpawnAvatarLogic += avatar =>
        {
            log.Debug("Created an avatar! {Avatar}", avatar.Self);
            avatar.OnAmmoCountChanged += () =>
            {
                log.Debug("Ammo Count changed to {AmmoCount}", avatar.Self.ammoCount);
                ammoCountChanged++;

                Assert.Equal(10 - ammoCountChanged, avatar.Self.ammoCount);
            };

            spawnedCalled++;
            log.Debug("Spawn Complete {Avatar}", avatar.Self);
            Assert.Equal(10, avatar.Self.ammoCount);

            avatar.OnFireButtonIsDownChanged += () =>
            {
                fireButtonChanged++;
                log.Info("FireButton changed {Count} {State}", fireButtonChanged, avatar.Self.fireButtonIsDown);
            };

            avatar.DoCastFireball += (position, direction) => { fireballFireCount++; };

            avatar.DoFireChainLightning += direction => { chainLightningCount++; };
        };

        Assert.Equal(0, spawnedCalled);
        Assert.Equal(10, spawnedHostAvatar.Self.ammoCount);

        hostGame.Host!.AssignPredictEntity(new RemoteEndpointId(2), new LocalPlayerIndex(0), spawnedEntity);

        const int maxIteration = 9;
        for (var iteration = 0; iteration < maxIteration; iteration++)
        {
            var now = new Milliseconds(initNow.ms + (iteration + 1) * 16);

            if (iteration == 3)
            {
                enqueue.Explode(new Position3(-200, 300, -400), 233);
            }

            var pressButtons = iteration >= 4;
            mockInput.PrimaryAbility = pressButtons;
            mockInput.SecondaryAbility = pressButtons;
            timeProvider.TimeInMs = now;

            clientGame.Update(now);
            hostGame.Update(now);
        }

        // Since snapshot queue has been starved on the client, the client playback is intentionally delayed
        // using a delta time of 19 ms instead of the normal 16 ms.
        // We add two ticks for the client to catch up.
        for (var clientIteration = 0; clientIteration < 2; clientIteration++)
        {
            var nowAfter = new Milliseconds(initNow.ms + (maxIteration + 1 + clientIteration) * 16);
            clientGame.Update(nowAfter);
        }

        Assert.Equal(1, spawnedCalled);
        Assert.Equal(1, fireballFireCount);
        Assert.Equal(1, fireButtonChanged);
        Assert.Equal(1, chainLightningCount);
        Assert.Equal(9, spawnedHostAvatar.Self.ammoCount);

        var clientAvatar = clientGame.EntityContainer.FetchEntity<AvatarLogicEntityInternal>(spawnedEntity.Id);

        log.Debug($"clientAvatar {clientAvatar.Self}\nhostAvatar {spawnedHostAvatar.Self}");
        Assert.Equal(spawnedHostAvatar.Self, clientAvatar.Self);
    }
}