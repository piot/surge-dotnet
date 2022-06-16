/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.TypeSerialization;

namespace Piot.Surge.Internal.Generated
{
public static class ArchetypeConstants
{
    public const ushort AvatarLogic = 1;
    public const ushort FireballLogic = 2;

}
public static class ArchetypeIdConstants
{
    public static readonly ArchetypeId AvatarLogic = new(ArchetypeConstants.AvatarLogic);
    public static readonly ArchetypeId FireballLogic = new(ArchetypeConstants.FireballLogic);

}
public class GeneratedEntityCreation : IEntityCreation
    {
 public IEntity CreateEntity(ArchetypeId archetypeId, EntityId entityId)
            {
                IGeneratedEntity generatedEntity = archetypeId.id switch
                {
        ArchetypeConstants.AvatarLogic => new AvatarLogicEntityInternal(),
        ArchetypeConstants.FireballLogic => new FireballLogicEntityInternal(),
            _ => throw new Exception($"unknown entity to create {archetypeId}"),

                };
                
                return new Entity(entityId, generatedEntity);
            }
        }
// --------------- EngineWorld ---------------
public class EngineWorld
    {
public Action<AvatarLogicEntity>? OnSpawnAvatarLogic;
public Action<FireballLogicEntity>? OnSpawnFireballLogic;

    }
public class NotifyEngineWorld
    {

        public static void NotifyCreation(IEntity entity, EngineWorld engineWorld)
        {
            switch (entity)
            {
case AvatarLogicEntityInternal internalEntity:
engineWorld.OnSpawnAvatarLogic?.Invoke(internalEntity.OutFacing);
    break;
case FireballLogicEntityInternal internalEntity:
engineWorld.OnSpawnFireballLogic?.Invoke(internalEntity.OutFacing);
    break;

                default:
                    throw new Exception("Internal error");
            }
        }
                
    }
// --------------- Internal Action Structs ---------------
public struct FireVolley : IAction
{
    public System.Numerics.Vector3 direction;

}
// --------------- Internal Action Implementation ---------------
public class AvatarLogicActions : Tests.ExampleGame.AvatarLogic.IAvatarLogicActions
{
    private readonly IActionsContainer actionsContainer;

    public AvatarLogicActions(IActionsContainer actionsContainer)
    {
        this.actionsContainer = actionsContainer;
    }
    public void FireVolley(System.Numerics.Vector3 direction)
    {
        actionsContainer.Add(new FireVolley(){direction = direction});
    }
}
public class AvatarLogicEntity
{
    public Action? OnDestroyed;
    public Action? OnSpawned;
    public Action? OnFireButtonIsDownChanged;

    public Action? OnAimingChanged;

    public Action? OnPositionChanged;

    public Action? OnAmmoCountChanged;

    public Action? OnFireCooldownChanged;

    public delegate void FireVolleyDelegate(System.Numerics.Vector3 direction);
    public FireVolleyDelegate? DoFireVolley;
    public FireVolleyDelegate? UnDoFireVolley;


}


public class AvatarLogicEntityInternal : IGeneratedEntity
{
    private readonly ActionsContainer actionsContainer = new();


    Tests.ExampleGame.AvatarLogic current;
    Tests.ExampleGame.AvatarLogic last;

    public Tests.ExampleGame.AvatarLogic Self => current;

 internal Tests.ExampleGame.AvatarLogic Current
            {
                set => current = value;
            }
     AvatarLogicEntity outFacing = new();
    public AvatarLogicEntity OutFacing => outFacing;

    public ArchetypeId ArchetypeId => ArchetypeIdConstants.AvatarLogic;
    public const ulong FireButtonIsDownMask = 0x00000001;
    public const ulong AimingMask = 0x00000002;
    public const ulong PositionMask = 0x00000004;
    public const ulong AmmoCountMask = 0x00000008;
    public const ulong FireCooldownMask = 0x00000010;


    public IAction[] Actions => actionsContainer.Actions.ToArray();
    
    public ILogic Logic => current;

    public void Overwrite()
    {
        last = current;
    }

    public void FireCreated()
    {
        outFacing.OnSpawned?.Invoke();
    }

    public void FireDestroyed()
    {
        outFacing.OnDestroyed?.Invoke();
    }

    public void DoAction(IAction action)
    {
        switch (action)
        {
            case FireVolley thing:
                outFacing.DoFireVolley?.Invoke(thing.direction);
            break;

    }
}
    public void UnDoAction(IAction action)
    {
        switch (action)
        {
            case FireVolley thing:
                outFacing.UnDoFireVolley?.Invoke(thing.direction);
            break;

    }
}
    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & AimingMask) != 0) AimingWriter.Write(current.aiming, writer);
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(current.position, writer);
        if ((serializeFlags & AmmoCountMask) != 0) writer.WriteUInt16(current.ammoCount);
        if ((serializeFlags & FireCooldownMask) != 0) writer.WriteUInt16(current.fireCooldown);
    }

    public void SerializeAll(IOctetWriter writer)
    {
        writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteUInt16(current.ammoCount);
        writer.WriteUInt16(current.fireCooldown);
    }

    public void Deserialize(ulong serializeFlags, IOctetReader reader)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) current.fireButtonIsDown = reader.ReadUInt8() != 0;
        if ((serializeFlags & AimingMask) != 0) current.aiming = AimingReader.Read(reader);
        if ((serializeFlags & PositionMask) != 0) current.position = Position3Reader.Read(reader);
        if ((serializeFlags & AmmoCountMask) != 0) current.ammoCount = reader.ReadUInt16();
        if ((serializeFlags & FireCooldownMask) != 0) current.fireCooldown = reader.ReadUInt16();
    }

    public void DeserializeAll(IOctetReader reader)
    {
        current.fireButtonIsDown = reader.ReadUInt8() != 0;
        current.aiming = AimingReader.Read(reader);
        current.position = Position3Reader.Read(reader);
        current.ammoCount = reader.ReadUInt16();
        current.fireCooldown = reader.ReadUInt16();
    }



    public void Tick()
    {
        var actions = new AvatarLogicActions(actionsContainer);
        current.Tick(actions);
    }

    public ulong Changes()
    {
        ulong mask = 0;

        if (current.fireButtonIsDown != last.fireButtonIsDown) mask |= FireButtonIsDownMask;
        if (current.aiming != last.aiming) mask |= AimingMask;
        if (current.position != last.position) mask |= PositionMask;
        if (current.ammoCount != last.ammoCount) mask |= AmmoCountMask;
        if (current.fireCooldown != last.fireCooldown) mask |= FireCooldownMask;

        return mask;

    }
    public void FireChanges(ulong serializeFlags)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) outFacing.OnFireButtonIsDownChanged?.Invoke();
        if ((serializeFlags & AimingMask) != 0) outFacing.OnAimingChanged?.Invoke();
        if ((serializeFlags & PositionMask) != 0) outFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & AmmoCountMask) != 0) outFacing.OnAmmoCountChanged?.Invoke();
        if ((serializeFlags & FireCooldownMask) != 0) outFacing.OnFireCooldownChanged?.Invoke();

    }

    public TypeInformation TypeInformation
    {
        get
        {
            return new TypeInformation(new TypeInformationField[]
            {
                new() { mask = FireButtonIsDownMask, name = new FieldName(nameof(current.fireButtonIsDown)), type = typeof(System.Boolean) },
                new() { mask = AimingMask, name = new FieldName(nameof(current.aiming)), type = typeof(Piot.Surge.Types.Aiming) },
                new() { mask = PositionMask, name = new FieldName(nameof(current.position)), type = typeof(Piot.Surge.Types.Position3) },
                new() { mask = AmmoCountMask, name = new FieldName(nameof(current.ammoCount)), type = typeof(System.UInt16) },
                new() { mask = FireCooldownMask, name = new FieldName(nameof(current.fireCooldown)), type = typeof(System.UInt16) },

        });
    }

}
}

// --------------- Internal Action Structs ---------------
public struct Explode : IAction
{

}
// --------------- Internal Action Implementation ---------------
public class FireballLogicActions : Tests.ExampleGame.IFireballLogicActions
{
    private readonly IActionsContainer actionsContainer;

    public FireballLogicActions(IActionsContainer actionsContainer)
    {
        this.actionsContainer = actionsContainer;
    }
    public void Explode()
    {
        actionsContainer.Add(new Explode(){});
    }
}
public class FireballLogicEntity
{
    public Action? OnDestroyed;
    public Action? OnSpawned;
    public Action? OnPositionChanged;

    public Action? OnVelocityChanged;

    public delegate void ExplodeDelegate();
    public ExplodeDelegate? DoExplode;
    public ExplodeDelegate? UnDoExplode;


}


public class FireballLogicEntityInternal : IGeneratedEntity
{
    private readonly ActionsContainer actionsContainer = new();


    Tests.ExampleGame.FireballLogic current;
    Tests.ExampleGame.FireballLogic last;

    public Tests.ExampleGame.FireballLogic Self => current;

 internal Tests.ExampleGame.FireballLogic Current
            {
                set => current = value;
            }
     FireballLogicEntity outFacing = new();
    public FireballLogicEntity OutFacing => outFacing;

    public ArchetypeId ArchetypeId => ArchetypeIdConstants.FireballLogic;
    public const ulong PositionMask = 0x00000001;
    public const ulong VelocityMask = 0x00000002;


    public IAction[] Actions => actionsContainer.Actions.ToArray();
    
    public ILogic Logic => current;

    public void Overwrite()
    {
        last = current;
    }

    public void FireCreated()
    {
        outFacing.OnSpawned?.Invoke();
    }

    public void FireDestroyed()
    {
        outFacing.OnDestroyed?.Invoke();
    }

    public void DoAction(IAction action)
    {
        switch (action)
        {
            case Explode:
                outFacing.DoExplode?.Invoke();
            break;

    }
}
    public void UnDoAction(IAction action)
    {
        switch (action)
        {
            case Explode:
                outFacing.UnDoExplode?.Invoke();
            break;

    }
}
    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(current.position, writer);
        if ((serializeFlags & VelocityMask) != 0) Velocity3Writer.Write(current.velocity, writer);
    }

    public void SerializeAll(IOctetWriter writer)
    {
        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void Deserialize(ulong serializeFlags, IOctetReader reader)
    {
        if ((serializeFlags & PositionMask) != 0) current.position = Position3Reader.Read(reader);
        if ((serializeFlags & VelocityMask) != 0) current.velocity = Velocity3Reader.Read(reader);
    }

    public void DeserializeAll(IOctetReader reader)
    {
        current.position = Position3Reader.Read(reader);
        current.velocity = Velocity3Reader.Read(reader);
    }



    public void Tick()
    {
        var actions = new FireballLogicActions(actionsContainer);
        current.Tick(actions);
    }

    public ulong Changes()
    {
        ulong mask = 0;

        if (current.position != last.position) mask |= PositionMask;
        if (current.velocity != last.velocity) mask |= VelocityMask;

        return mask;

    }
    public void FireChanges(ulong serializeFlags)
    {
        if ((serializeFlags & PositionMask) != 0) outFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & VelocityMask) != 0) outFacing.OnVelocityChanged?.Invoke();

    }

    public TypeInformation TypeInformation
    {
        get
        {
            return new TypeInformation(new TypeInformationField[]
            {
                new() { mask = PositionMask, name = new FieldName(nameof(current.position)), type = typeof(Piot.Surge.Types.Position3) },
                new() { mask = VelocityMask, name = new FieldName(nameof(current.velocity)), type = typeof(Piot.Surge.Types.Velocity3) },

        });
    }

}
}


} // Namespace
