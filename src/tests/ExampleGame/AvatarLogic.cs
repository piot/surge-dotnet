/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using Piot.Surge;
using Piot.Surge.Types;

namespace Tests.ExampleGame;

[Logic]
public struct AvatarLogic : ILogic
{
    [InputSource(BindName = nameof(GameInput.fire))]
    public bool fireButtonIsDown;

    [InputSource(BindName = nameof(GameInput.aiming))]
    public Aiming aiming;

    public Position3 position;

    public ushort ammoCount;
    public ushort fireCooldown;

    public override string ToString()
    {
        return $"AvatarLogic {ammoCount}";
    }

    public interface IAvatarLogicActions : ILogicActions
    {
        public void FireVolley(Vector3 direction);
    }

    public void Tick(IAvatarLogicActions commands)
    {
        if (fireButtonIsDown && fireCooldown == 0)
            if (ammoCount > 0)
            {
                ammoCount--;
                fireCooldown = 30;
                commands.FireVolley(aiming.ToDirection);
            }

        position += new Position3(3, 0, 0);
    }
}