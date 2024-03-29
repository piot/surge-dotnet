/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Compress;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Transport;
using Tests.Surge.ExampleGame;

namespace Tests.ExampleGame;

public sealed class Game
{
    readonly FetchInput inputFetch = new();
    readonly ILog log;
    readonly WorldWithGhostCreator world;

    public Game(ITransport transport, IMultiCompressor compression, bool isHosting, ILog log)
    {
        this.log = log;
        var now = new TimeMs(0);
        var delta = new FixedDeltaTimeMs(16);

        var entityCreation = new GeneratedEntityGhostCreator();
        GeneratedNotifyEntityCreation = new();
        world = new(entityCreation, GeneratedNotifyEntityCreation, GeneratedNotifyEntityCreation, isHosting);
        var generatedEventTarget = new GeneratedEventProcessor(new ShortEvents(log.SubLog("ShortEvents")));

        if (isHosting)
        {
            var hostInfo = new HostInfo
            {
                hostTransport = transport,
                compression = compression,
                compressorIndex = DefaultMultiCompressor.DeflateCompressionIndex,
                authoritativeWorld = world,
                onConnectionCreated = OnConnectionCreated,
                targetDeltaTimeMs = delta,
                now = now
            };
            Host = new(hostInfo, log.SubLog("Host"));
        }
        else
        {
            var gameInputFetch = new GeneratedInputPackFetch();
            gameInputFetch.GameSpecificInputFetch = inputFetch.ReadFromDevice;
            var clientInfo = new ClientInfo
            {
                now = now,
                targetDeltaTimeMs = delta,
                worldWithGhostCreator = world,
                eventProcessor = generatedEventTarget,
                assignedTransport = transport,
                compression = compression,
                fetch = gameInputFetch,
                snapshotPlaybackNotify = new MockPlaybackNotify()
            };

            Client = new(clientInfo, log.SubLog("Client"))
            {
                UsePrediction = false
            };
        }

        GeneratedHostEntitySpawner = new(world, GeneratedNotifyEntityCreation);

        GeneratedNotifyEntityCreation.OnSpawnFireballLogic += (fireballEntity, fireball) =>
        {
            log.Debug($"a fireball has been spawned {fireball.Self}");

            fireball.OnPositionChanged +=
                () => log.Debug("Fireball position changed {Position}", fireball.Self.position);
        };

        GeneratedNotifyEntityCreation.OnSpawnAvatarLogic += (avatarEntity, avatar) =>
        {
            log.Debug("Avatar {Avatar} was spawned", avatar);

            avatar.OnPositionChanged += () => log.Debug("Avatar position changed {Position}", avatar.Self.position);
            avatar.OnManaAmountChanged += () =>
                log.Debug("Mana has changed {Mana} in {RollMode}", avatar.Self.manaAmount, avatar.RollMode);
            avatar.DoCastFireball += (position, direction) =>
            {
                log.Debug("Play effect for avatar casting a fireball");
                if (!world.IsAuthoritative)
                {
                    return;
                }

                log.Debug("We are on the host, spawn fireball");
                var fireballLogic = new FireballLogic
                {
                    position = position,
                    velocity = new(200, 300, 400)
                };

                GeneratedHostEntitySpawner.SpawnFireballLogic(fireballLogic);
            };

            avatar.OnPostUpdate += () =>
            {
                var self = avatar.Self;
                if (self.manaAmount < 10)
                {
                }
            };
        };
    }

    public Host? Host { get; }
    public Client? Client { get; }

    public IEntityContainer EntityContainer => world;
    public GeneratedHostEntitySpawner GeneratedHostEntitySpawner { get; }

    public GeneratedNotifyEntityCreation GeneratedNotifyEntityCreation { get; }

    void OnConnectionCreated(ConnectionToClient connectionToClient)
    {
        // intentionally blank
    }

    public void Update(TimeMs now)
    {
        log.Debug("Update");
        Client?.Update(now);
        Host?.Update(now);
    }

    public class FetchInput
    {
        public GameInput ReadFromDevice(LocalPlayerIndex localPlayerIndex)
        {
            return new()
            {
                primaryAbility = true,
                secondaryAbility = false
            };
        }
    }
}