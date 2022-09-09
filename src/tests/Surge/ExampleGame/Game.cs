/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Compress;
using Piot.Surge.Internal.Generated;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Surge.Types;
using Piot.Transport;

namespace Tests.ExampleGame;

public class Game
{
    private readonly ILog log;
    private readonly WorldWithGhostCreator world;

    public Game(ITransport transport, IMultiCompressor compression, bool isHosting, ILog log)
    {
        this.log = log;
        var now = new Milliseconds(0);
        var delta = new Milliseconds(16);

        var entityCreation = new GeneratedEntityGhostCreator();
        GeneratedNotifyEntityCreation = new GeneratedNotifyEntityCreation();
        world = new(entityCreation, GeneratedNotifyEntityCreation, isHosting);
        var generatedEventTarget = new MockEventProcessorWithCreate(log);

        if (isHosting)
        {
            Host = new Host(transport, compression, DefaultMultiCompressor.DeflateCompressionIndex,
                world, now, log.SubLog("Host"));
        }
        else
        {
            Client = new(log.SubLog("Client"), now, delta, world, generatedEventTarget,
                transport, compression, new GeneratedInputFetch());
        }

        GeneratedHostEntitySpawner = new GeneratedHostEntitySpawner(world, GeneratedNotifyEntityCreation);

        GeneratedNotifyEntityCreation.OnSpawnFireballLogic += fireball =>
        {
            log.Debug($"a fireball has been spawned {fireball.Self}");

            fireball.OnPositionChanged +=
                () => log.Debug("Fireball position changed {Position}", fireball.Self.position);
        };

        GeneratedNotifyEntityCreation.OnSpawnAvatarLogic += avatar =>
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
                    velocity = new Velocity3(200, 300, 400)
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

    public void Update(Milliseconds now)
    {
        log.Debug("Update");
        Client?.Update(now);
        Host?.Update(now);
    }
}