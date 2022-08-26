/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Internal.Generated;
using Piot.Surge.Pulse.Client;
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

        var entityCreation = new GeneratedEntityCreation();
        var generatedWorld = new GeneratedEngineWorld();

        generatedWorld.OnSpawnAvatarLogic += avatar =>
        {
            log.Debug("Avatar {Avatar} was spawned", avatar);

            avatar.OnPositionChanged += () => log.Debug("Avatar position changed");
            avatar.OnManaAmountChanged += () =>
                log.Debug("Mana has changed {Mana} in {RollMode}", avatar.Self.manaAmount, avatar.RollMode);
        };

        client = new(log, now, delta, entityCreation, generatedWorld,
            transport, new GeneratedInputFetch());
    }

    public void Update(Milliseconds now)
    {
        log.Debug("Update");
        client.Update(now);
    }
}