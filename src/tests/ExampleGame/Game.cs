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
    private readonly Client? client;
    private readonly Host? host;
    private readonly ILog log;
    private readonly WorldWithGhostCreator world;

    public Game(ITransport transport, IMultiCompressor compression, bool isHosting, ILog log)
    {
        this.log = log;
        var now = new Milliseconds(0);
        var delta = new Milliseconds(16);

        var entityCreation = new GeneratedEntityGhostCreator();
        GeneratedNotifyWorld = new GeneratedEngineWorld();
        world = new(entityCreation, GeneratedNotifyWorld);

        if (isHosting)
        {
            host = new Host(transport, compression, DefaultMultiCompressor.DeflateCompressionIndex,
                world, now, log);
        }
        else
        {
            client = new(log, now, delta, world,
                transport, compression, new GeneratedInputFetch());
        }

        GeneratedEngineSpawner = new GeneratedEngineSpawner(world);

        GeneratedNotifyWorld.OnSpawnAvatarLogic += avatar =>
        {
            log.Debug("Avatar {Avatar} was spawned", avatar);

            avatar.OnPositionChanged += () => log.Debug("Avatar position changed");
            avatar.OnManaAmountChanged += () =>
                log.Debug("Mana has changed {Mana} in {RollMode}", avatar.Self.manaAmount, avatar.RollMode);
            avatar.DoCastFireball += (position, direction) =>
            {
                log.Debug("Play cast effect!");
                if (!world.IsAuthoritative)
                {
                    return;
                }

                var fireballLogic = new FireballLogic
                {
                    position = position,
                    velocity = new Velocity3((int)direction.X, (int)direction.Y, (int)direction.Z)
                };
                GeneratedEngineSpawner.SpawnFireballLogic(fireballLogic);
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

    public IEntityContainer EntityContainer => world;
    public GeneratedEngineSpawner GeneratedEngineSpawner { get; }

    public GeneratedEngineWorld GeneratedNotifyWorld { get; }

    public void Update(Milliseconds now)
    {
        log.Debug("Update");
        client?.Update(now);
        host?.Update(now);
    }
}