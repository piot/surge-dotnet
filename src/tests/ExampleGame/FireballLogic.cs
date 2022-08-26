/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.Types;

namespace Tests.ExampleGame;

public interface IFireballLogicActions : ILogicActions
{
    public void Explode();
}

[Logic]
public struct FireballLogic : ILogic
{
    public Position3 position;
    public Velocity3 velocity;

    public void Tick(SimulationMode mode, IFireballLogicActions commands)
    {
        position += velocity;
        if (Math.Abs(position.x) > 2000)
        {
            commands.Explode();
        }
    }
}