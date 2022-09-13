/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.Types;

namespace Benchmark.Surge.ExampleGame;

public interface IFireballLogicActions : ILogicActions
{
    public void Explode();
}

[Logic]
public struct BenchmarkFireballLogic : ILogic
{
    public Position3 position;
    public Velocity3 velocity;

    public void Tick(IFireballLogicActions commands)
    {
        position += velocity;
        if (Math.Abs(position.x) > 2000)
        {
            commands.Explode();
        }
    }

    public override string ToString()
    {
        return $"[fireball {position} vel:{velocity}]";
    }
}