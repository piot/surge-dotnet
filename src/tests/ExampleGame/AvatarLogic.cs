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
    public bool fireButtonIsDown;
    public bool castButtonIsDown;
    public Aiming aiming;

    public Position3 position;

    public ushort ammoCount;
    public ushort fireCooldown;

    public ushort manaAmount;
    public ushort castCooldown;


    public void SetInput(GameInput input)
    {
        fireButtonIsDown = input.primaryAbility;
        castButtonIsDown = input.secondaryAbility;
        aiming = input.aiming;
    }

    public override string ToString()
    {
        return $"AvatarLogic {ammoCount}";
    }

    public interface IAvatarLogicActions : ILogicActions
    {
        /// <summary>
        ///     Fires very fast-moving Chain Lightning
        /// </summary>
        /// <param name="direction"></param>
        public void FireChainLightning(Vector3 direction);

        /// <summary>
        ///     Casts a fireball in the given direction. Starts playing the effects.
        ///     If <paramref name="isAuthoritative" /> is true, then fireball is spawned as well.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="isAuthoritative"></param>
        public void CastFireball(Position3 position, Vector3 direction);
    }

    private void Fire(IAvatarLogicActions commands)
    {
        ammoCount--;
        fireCooldown = 30;
        var fakeAiming = new Vector3
        {
            X = position.x,
            Y = position.y,
            Z = position.z
        };
        commands.FireChainLightning(fakeAiming); // aiming.ToDirection
    }

    private void Cast(IAvatarLogicActions commands)
    {
        manaAmount -= 10;

        var castDirection = aiming.ToDirection;
        commands.CastFireball(position, castDirection);
        castCooldown = 40;
    }

    private bool CanFire => fireCooldown == 0 && ammoCount > 0;

    private bool ShouldFire => fireButtonIsDown && CanFire;

    private bool CanCast => castCooldown == 0 && manaAmount > 10;
    private bool ShouldCast => castButtonIsDown && CanCast;

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

        if (castCooldown > 0)
        {
            castCooldown--;
        }
    }

    public void Tick(IAvatarLogicActions commands)
    {
        TickDownCoolDowns();

        if (ShouldFire)
        {
            Fire(commands);
        }

        if (ShouldCast)
        {
            Cast(commands);
        }

        AlwaysMoveRight();
    }
}