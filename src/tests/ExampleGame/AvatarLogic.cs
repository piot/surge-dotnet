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

    private void Fire(IAvatarLogicActions commands)
    {
        ammoCount--;
        fireCooldown = 30;
        var fakeAiming = new Vector3
        {
            X = position.x,
            Y = position.y,
            Z = position.z,
        };
        commands.FireVolley(fakeAiming); // aiming.ToDirection
    }

    private bool CanFire => fireCooldown == 0 && ammoCount > 0;

    private bool ShouldFire => fireButtonIsDown && CanFire;

    private void AlwaysMoveRight()
    {
        position += new Position3(3, 0, 0);
    }

    private void TickDownCoolDowns()
    {
        if (fireCooldown > 0)
        {
            fireCooldown--;
        }
    }
    
    public void Tick(IAvatarLogicActions commands)
    {
        TickDownCoolDowns();

        if (ShouldFire)
        {
            Fire(commands);
        }

        AlwaysMoveRight();
    }
}