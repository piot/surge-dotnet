/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Compress;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Transport;
using Piot.Transport.Memory;
using Tests.ExampleGame;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class GameSync
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public GameSync(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
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

        var clientGame = new Game(clientTransport, multiCompressor, false, log.SubLog("client"));
        var hostGame = new Game(hostTransport, multiCompressor, true, log.SubLog("host"));


        var mockInput = new MockInputFetch();

        clientGame.Client!.InputFetch = mockInput;

        var entitySpawner = hostGame.GeneratedEngineSpawner;
        var (spawnedEntity, spawnedHostAvatar) = entitySpawner.SpawnAvatarLogic(new AvatarLogic
        {
            fireButtonIsDown = false,
            castButtonIsDown = false,
            aiming = default,
            position = default,
            ammoCount = 10,
            fireCooldown = 0,
            manaAmount = 16,
            castCooldown = 0,
            jumpTime = 0
        });
        log.Info("Spawned entity {Entity}", spawnedHostAvatar);

        var spawnedCalled = 0;
        var fireButtonChanged = 0;
        var fireballFireCount = 0;
        var ammoCountChanged = 0;
        var chainLightningCount = 0;

        clientGame.GeneratedNotifyWorld.OnSpawnAvatarLogic += avatar =>
        {
            log.Debug("Created an avatar! {Avatar}", avatar.Self);
            avatar.OnAmmoCountChanged += () =>
            {
                log.Debug("Ammo Count changed {AmmoCount}", avatar.Self.ammoCount);
                ammoCountChanged++;

                Assert.Equal(11 - ammoCountChanged, avatar.Self.ammoCount);
            };

            avatar.OnSpawned += () =>
            {
                spawnedCalled++;
                log.Debug("Spawn Complete {Avatar}", avatar.Self);
                Assert.Equal(10, avatar.Self.ammoCount);
            };

            avatar.OnFireButtonIsDownChanged += () => { fireButtonChanged++; };

            avatar.DoCastFireball += (position, direction) => { fireballFireCount++; };

            avatar.DoFireChainLightning += direction => { chainLightningCount++; };
        };

        Assert.Equal(0, spawnedCalled);
        Assert.Equal(10, spawnedHostAvatar.Self.ammoCount);


        hostGame.Host!.AssignPredictEntity(new RemoteEndpointId(2), new LocalPlayerIndex(0), spawnedEntity);

        const int maxIteration = 8;
        for (var iteration = 0; iteration < maxIteration; iteration++)
        {
            var now = new Milliseconds(initNow.ms + (iteration + 1) * 16);

            var pressButtons = iteration >= 3;
            mockInput.PrimaryAbility = pressButtons;
            mockInput.SecondaryAbility = pressButtons;
            timeProvider.TimeInMs = now;

            clientGame.Update(now);
            hostGame.Update(now);
        }

        for (var clientIteration = 0; clientIteration < 2; clientIteration++)
        {
            var nowAfter = new Milliseconds(initNow.ms + (maxIteration + 1 + clientIteration) * 16);
            clientGame.Update(nowAfter);
        }

        Assert.Equal(1, spawnedCalled);
        Assert.Equal(9, spawnedHostAvatar.Self.ammoCount);
        Assert.Equal(1, chainLightningCount);
        Assert.Equal(1, fireballFireCount);
        Assert.Equal(1, fireButtonChanged);

        var clientAvatar = clientGame.EntityContainer.FetchEntity<AvatarLogicEntityInternal>(spawnedEntity.Id);

        log.Debug($"clientAvatar {clientAvatar.Self}\nhostAvatar {spawnedHostAvatar.Self}");
        Assert.Equal(clientAvatar.Self, spawnedHostAvatar.Self);
    }

    public class MockInputFetch : IInputPackFetch
    {
        private bool primaryAbility;

        private bool secondaryAbility;

        public bool PrimaryAbility
        {
            set => primaryAbility = value;
        }

        public bool SecondaryAbility
        {
            set => secondaryAbility = value;
        }

        public ReadOnlySpan<byte> Fetch(LocalPlayerIndex playerIndex)
        {
            var input = new GameInput
            {
                aiming = default,
                primaryAbility = primaryAbility,
                secondaryAbility = secondaryAbility,
                tertiaryAbility = false,
                ultimateAbility = false,
                desiredMovement = default
            };
            var writer = new OctetWriter(30);

            GameInputWriter.Write(writer, input);

            return writer.Octets;
        }
    }
}