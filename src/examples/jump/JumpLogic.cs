/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.Types;

namespace Jump;

public class JumperLogic : ILogic
{
    public Position3 aiming = new();
    public bool isFiring;
    public Action? onIsFiringChanged;
    public Action? onPositionChanged;
    public Position3 position;

    public JumperLogic()
    {
        onPositionChanged += OnPositionChanged;
    }

    private void OnPositionChanged()
    {
        Console.WriteLine("Position was changed");
    }

    public void Tick()
    {
        position = position with { x = position.x + 10 };
        //if (isFiring && isSimulator) commands.Spawn(new RocketLogic { position = position, damage = 120 });
    }
}

public class RocketLogic : ILogic
{
    public int damage;
    public Position3 position = new();


    public void Tick()
    {
        throw new NotImplementedException();
    }
}