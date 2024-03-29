/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;
using Piot.Surge;
using Piot.Surge.Types;

namespace Benchmark.Surge.ExampleGame;

[Logic]
public struct BenchmarkAvatarLogic : ILogic
{
    public bool fireButtonIsDown;
    public bool castButtonIsDown;
    public Aiming aiming;

    public Position3 position;

    public ushort ammoCount;
    public ushort fireCooldown;

    public ushort manaAmount;
    public ushort castCooldown;

    public ushort jumpTime;


    public void SetInput(BenchmarkGameInput input)
    {
        fireButtonIsDown = input.primaryAbility;
        castButtonIsDown = input.secondaryAbility;
        aiming = input.aiming;
    }

    public override string ToString()
    {
        return
            $"[BenchmarkAvatarLogic pos:{position} ammo:{ammoCount} fireButton:{fireButtonIsDown} castButton:{castButtonIsDown}]";
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
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        public void CastFireball(Position3 position, UnitVector3 direction);
    }

    void Fire(IAvatarLogicActions commands)
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

    void Cast(IAvatarLogicActions commands)
    {
        manaAmount -= 10;

        var castDirection = aiming.ToDirection;
        commands.CastFireball(position, castDirection);
        castCooldown = 40;
    }

    bool CanFire => fireCooldown == 0 && ammoCount > 0;

    bool ShouldFire => fireButtonIsDown && CanFire;

    bool CanCast => castCooldown == 0 && manaAmount > 10;
    bool ShouldCast => castButtonIsDown && CanCast;

    void AlwaysMoveRight()
    {
        position += new Position3(300, 0, 0);
    }

    void TickDownCoolDowns()
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