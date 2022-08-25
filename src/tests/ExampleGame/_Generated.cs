// Code generated by Surge Generator. DO NOT EDIT
// <auto-generated> This file has been auto generated. </auto-generated>
#nullable enable


using Piot.Surge.FastTypeInformation;
using Piot.Flood;
using Piot.Surge.TypeSerialization;
using Piot.Surge.LogicalInput;
using Piot.Surge.LocalPlayer;

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
public static class GameInputReader
{
    public static Tests.ExampleGame.GameInput Read(IOctetReader reader)
    {
        return new()
        {
             aiming = AimingReader.Read(reader),
             primaryAbility = reader.ReadUInt8() != 0,
             secondaryAbility = reader.ReadUInt8() != 0,
             tertiaryAbility = reader.ReadUInt8() != 0,
             ultimateAbility = reader.ReadUInt8() != 0,
        }; // end of new
}

}
public static class GameInputWriter
{
    public static void Write(IOctetWriter writer, Tests.ExampleGame.GameInput input)
    {
       AimingWriter.Write(input.aiming, writer);
       writer.WriteUInt8(input.primaryAbility ? (byte)1 : (byte)0);
       writer.WriteUInt8(input.secondaryAbility ? (byte)1 : (byte)0);
       writer.WriteUInt8(input.tertiaryAbility ? (byte)1 : (byte)0);
       writer.WriteUInt8(input.ultimateAbility ? (byte)1 : (byte)0);

}

}


public class GeneratedInputFetch : IInputPackFetch
{
    public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
    {
        var gameInput = Tests.ExampleGame.GameInputFetch.ReadFromDevice(index); // Found from scanning
        var writer = new OctetWriter(256);
        GameInputWriter.Write(writer, gameInput);

        return writer.Octets;
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


public class AvatarLogicEntityInternal : IGeneratedEntity, IInputDeserialize
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
        actionsContainer.Clear();
    }

    public void FireCreated()
    {
        outFacing.OnSpawned?.Invoke();
    }

    public void FireDestroyed()
    {
        outFacing.OnDestroyed?.Invoke();
    }



            public void SetInput(IOctetReader reader)
            {
                current.SetInput(GameInputReader.Read(reader));
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

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) writer.WriteUInt8(last.fireButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & AimingMask) != 0) AimingWriter.Write(last.aiming, writer);
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(last.position, writer);
        if ((serializeFlags & AmmoCountMask) != 0) writer.WriteUInt16(last.ammoCount);
        if ((serializeFlags & FireCooldownMask) != 0) writer.WriteUInt16(last.fireCooldown);
    }

    public void SerializeAll(IOctetWriter writer)
    {
        writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteUInt16(current.ammoCount);
        writer.WriteUInt16(current.fireCooldown);
    }

    public void SerializeCorrectionState(IOctetWriter writer)
    {
    }

    public void DeserializeCorrectionState(IOctetReader reader)
    {
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
                new() { mask = FireButtonIsDownMask, name = new (nameof(current.fireButtonIsDown)), type = typeof(System.Boolean) },
                new() { mask = AimingMask, name = new (nameof(current.aiming)), type = typeof(Piot.Surge.Types.Aiming) },
                new() { mask = PositionMask, name = new (nameof(current.position)), type = typeof(Piot.Surge.Types.Position3) },
                new() { mask = AmmoCountMask, name = new (nameof(current.ammoCount)), type = typeof(System.UInt16) },
                new() { mask = FireCooldownMask, name = new (nameof(current.fireCooldown)), type = typeof(System.UInt16) },

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
        actionsContainer.Clear();
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

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(last.position, writer);
        if ((serializeFlags & VelocityMask) != 0) Velocity3Writer.Write(last.velocity, writer);
    }

    public void SerializeAll(IOctetWriter writer)
    {
        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void SerializeCorrectionState(IOctetWriter writer)
    {
    }

    public void DeserializeCorrectionState(IOctetReader reader)
    {
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
                new() { mask = PositionMask, name = new (nameof(current.position)), type = typeof(Piot.Surge.Types.Position3) },
                new() { mask = VelocityMask, name = new (nameof(current.velocity)), type = typeof(Piot.Surge.Types.Velocity3) },

        });
    }

}
}


} // Namespace
