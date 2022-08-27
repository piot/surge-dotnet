/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Internal.Generated;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Types;
using Piot.Transport;

namespace Tests.ExampleGame;

public class Game
{
    private readonly Client client;
    private readonly ILog log;

    public Game(ITransport transport, ILog log)
    {
        this.log = log;
        var now = new Milliseconds(0);
        var delta = new Milliseconds(16);

        var entityCreation = new GeneratedEntityGhostCreator();
        var generatedWorld = new GeneratedEngineWorld();
        var worldWithGhostCreator = new WorldWithGhostCreator(entityCreation, generatedWorld);

        client = new(log, now, delta, worldWithGhostCreator,
            transport, new GeneratedInputFetch());
        var world = client.World;

        var generatedSpawner = new GeneratedEngineSpawner(worldWithGhostCreator);

        generatedWorld.OnSpawnAvatarLogic += avatar =>
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
                generatedSpawner.SpawnFireballLogic(fireballLogic);
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

    public void Update(Milliseconds now)
    {
        log.Debug("Update");
        client.Update(now);
    }
}