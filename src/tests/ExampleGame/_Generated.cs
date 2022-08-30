// Code generated by Surge generator. DO NOT EDIT.
// <auto-generated /> This file has been auto generated.
#nullable enable


using Piot.Surge.FastTypeInformation;
using Piot.Flood;
using Piot.Surge.Type.Serialization;
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
public class GeneratedEntityGhostCreator : IEntityGhostCreator
    {
 public IEntity CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
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
public class GeneratedEngineWorld : INotifyWorld
    {
public Action<AvatarLogicEntity>? OnSpawnAvatarLogic;
public Action<FireballLogicEntity>? OnSpawnFireballLogic;

        void INotifyWorld.NotifyCreation(IGeneratedEntity entity)
        {
            switch (entity)
            {
case AvatarLogicEntityInternal internalEntity:
OnSpawnAvatarLogic?.Invoke(internalEntity.OutFacing);
    break;
case FireballLogicEntityInternal internalEntity:
OnSpawnFireballLogic?.Invoke(internalEntity.OutFacing);
    break;

                default:
                    throw new Exception("Internal error");
            }
        }
                
}
public class GeneratedEngineSpawner
{

    private readonly IAuthoritativeEntityContainer container;

    public GeneratedEngineSpawner(IAuthoritativeEntityContainer container)
    {
        this.container = container;
    }

    public IEntity SpawnAvatarLogic(Tests.ExampleGame.AvatarLogic logic)
    { 
        return container.SpawnEntity(new AvatarLogicEntityInternal
        {
            Current = logic
        });
     }

    public IEntity SpawnFireballLogic(Tests.ExampleGame.FireballLogic logic)
    { 
        return container.SpawnEntity(new FireballLogicEntityInternal
        {
            Current = logic
        });
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
             desiredMovement = Velocity2Reader.Read(reader),
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
       Velocity2Writer.Write(input.desiredMovement, writer);

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
public struct FireChainLightning : IAction
{
    public System.Numerics.Vector3 direction;

}
public struct CastFireball : IAction
{
    public Piot.Surge.Types.Position3 position;
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
    public void FireChainLightning(System.Numerics.Vector3 direction)
    {
        actionsContainer.Add(new FireChainLightning(){direction = direction});
    }    public void CastFireball(Piot.Surge.Types.Position3 position, System.Numerics.Vector3 direction)
    {
        actionsContainer.Add(new CastFireball(){position = position, direction = direction});
    }
}
public class AvatarLogicEntity
{
    public EntityRollMode RollMode => internalEntity.RollMode;

    private readonly AvatarLogicEntityInternal internalEntity;
    internal AvatarLogicEntity(AvatarLogicEntityInternal internalEntity)
    {
        this.internalEntity = internalEntity;
    }

    public Tests.ExampleGame.AvatarLogic Self => internalEntity.Self;

    public Action? OnDestroyed;
    public Action? OnSpawned;

    public Action? OnPostUpdate;
    public Action? OnFireButtonIsDownChanged;

    public Action? OnCastButtonIsDownChanged;

    public Action? OnAimingChanged;

    public Action? OnPositionChanged;

    public Action? OnAmmoCountChanged;

    public Action? OnFireCooldownChanged;

    public Action? OnManaAmountChanged;

    public Action? OnCastCooldownChanged;

    public delegate void FireChainLightningDelegate(System.Numerics.Vector3 direction);
    public FireChainLightningDelegate? DoFireChainLightning;
    public FireChainLightningDelegate? UnDoFireChainLightning;

    public delegate void CastFireballDelegate(Piot.Surge.Types.Position3 position, System.Numerics.Vector3 direction);
    public CastFireballDelegate? DoCastFireball;
    public CastFireballDelegate? UnDoCastFireball;


}


public class AvatarLogicEntityInternal : IGeneratedEntity, IInputDeserialize
{
    private readonly ActionsContainer actionsContainer = new();


public AvatarLogicEntityInternal()
    {
        outFacing = new(this);
    }
    Tests.ExampleGame.AvatarLogic current;
    Tests.ExampleGame.AvatarLogic last;

    public Tests.ExampleGame.AvatarLogic Self => current;

        public EntityRollMode RollMode { get; set; }

 internal Tests.ExampleGame.AvatarLogic Current
            {
                set => current = value;
            }
     AvatarLogicEntity outFacing;
    public AvatarLogicEntity OutFacing => outFacing;

    public ArchetypeId ArchetypeId => ArchetypeIdConstants.AvatarLogic;
    public const ulong FireButtonIsDownMask = 0x00000001;
    public const ulong CastButtonIsDownMask = 0x00000002;
    public const ulong AimingMask = 0x00000004;
    public const ulong PositionMask = 0x00000008;
    public const ulong AmmoCountMask = 0x00000010;
    public const ulong FireCooldownMask = 0x00000020;
    public const ulong ManaAmountMask = 0x00000040;
    public const ulong CastCooldownMask = 0x00000080;


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
            case FireChainLightning thing:
                outFacing.DoFireChainLightning?.Invoke(thing.direction);
            break;
            case CastFireball thing:
                outFacing.DoCastFireball?.Invoke(thing.position, thing.direction);
            break;

    }
}
    public void UnDoAction(IAction action)
    {
        switch (action)
        {
            case FireChainLightning thing:
                outFacing.UnDoFireChainLightning?.Invoke(thing.direction);
            break;
            case CastFireball thing:
                outFacing.UnDoCastFireball?.Invoke(thing.position, thing.direction);
            break;

    }
}
    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & CastButtonIsDownMask) != 0) writer.WriteUInt8(current.castButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & AimingMask) != 0) AimingWriter.Write(current.aiming, writer);
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(current.position, writer);
        if ((serializeFlags & AmmoCountMask) != 0) writer.WriteUInt16(current.ammoCount);
        if ((serializeFlags & FireCooldownMask) != 0) writer.WriteUInt16(current.fireCooldown);
        if ((serializeFlags & ManaAmountMask) != 0) writer.WriteUInt16(current.manaAmount);
        if ((serializeFlags & CastCooldownMask) != 0) writer.WriteUInt16(current.castCooldown);
    }

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
        if ((serializeFlags & FireButtonIsDownMask) != 0) writer.WriteUInt8(last.fireButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & CastButtonIsDownMask) != 0) writer.WriteUInt8(last.castButtonIsDown ? (byte)1 : (byte)0);
        if ((serializeFlags & AimingMask) != 0) AimingWriter.Write(last.aiming, writer);
        if ((serializeFlags & PositionMask) != 0) Position3Writer.Write(last.position, writer);
        if ((serializeFlags & AmmoCountMask) != 0) writer.WriteUInt16(last.ammoCount);
        if ((serializeFlags & FireCooldownMask) != 0) writer.WriteUInt16(last.fireCooldown);
        if ((serializeFlags & ManaAmountMask) != 0) writer.WriteUInt16(last.manaAmount);
        if ((serializeFlags & CastCooldownMask) != 0) writer.WriteUInt16(last.castCooldown);
    }

    public void SerializeAll(IOctetWriter writer)
    {
        writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        writer.WriteUInt8(current.castButtonIsDown ? (byte)1 : (byte)0);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteUInt16(current.ammoCount);
        writer.WriteUInt16(current.fireCooldown);
        writer.WriteUInt16(current.manaAmount);
        writer.WriteUInt16(current.castCooldown);
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
        if ((serializeFlags & CastButtonIsDownMask) != 0) current.castButtonIsDown = reader.ReadUInt8() != 0;
        if ((serializeFlags & AimingMask) != 0) current.aiming = AimingReader.Read(reader);
        if ((serializeFlags & PositionMask) != 0) current.position = Position3Reader.Read(reader);
        if ((serializeFlags & AmmoCountMask) != 0) current.ammoCount = reader.ReadUInt16();
        if ((serializeFlags & FireCooldownMask) != 0) current.fireCooldown = reader.ReadUInt16();
        if ((serializeFlags & ManaAmountMask) != 0) current.manaAmount = reader.ReadUInt16();
        if ((serializeFlags & CastCooldownMask) != 0) current.castCooldown = reader.ReadUInt16();
    }

    public void DeserializeAll(IOctetReader reader)
    {
        current.fireButtonIsDown = reader.ReadUInt8() != 0;
        current.castButtonIsDown = reader.ReadUInt8() != 0;
        current.aiming = AimingReader.Read(reader);
        current.position = Position3Reader.Read(reader);
        current.ammoCount = reader.ReadUInt16();
        current.fireCooldown = reader.ReadUInt16();
        current.manaAmount = reader.ReadUInt16();
        current.castCooldown = reader.ReadUInt16();
    }



    public void Tick()
    {
        var actions = new AvatarLogicActions(actionsContainer);
        current.Tick(actions);
    }

    public ulong Changes()
    {
        ulong mask = 0;

        // ReSharper disable EnforceIfStatementBraces

        if (current.fireButtonIsDown != last.fireButtonIsDown) mask |= FireButtonIsDownMask;
        if (current.castButtonIsDown != last.castButtonIsDown) mask |= CastButtonIsDownMask;
        if (current.aiming != last.aiming) mask |= AimingMask;
        if (current.position != last.position) mask |= PositionMask;
        if (current.ammoCount != last.ammoCount) mask |= AmmoCountMask;
        if (current.fireCooldown != last.fireCooldown) mask |= FireCooldownMask;
        if (current.manaAmount != last.manaAmount) mask |= ManaAmountMask;
        if (current.castCooldown != last.castCooldown) mask |= CastCooldownMask;

        return mask;
        // ReSharper restore EnforceIfStatementBraces

    }
    public void FireChanges(ulong serializeFlags)
    {
        // ReSharper disable EnforceIfStatementBraces

        if ((serializeFlags & FireButtonIsDownMask) != 0) outFacing.OnFireButtonIsDownChanged?.Invoke();
        if ((serializeFlags & CastButtonIsDownMask) != 0) outFacing.OnCastButtonIsDownChanged?.Invoke();
        if ((serializeFlags & AimingMask) != 0) outFacing.OnAimingChanged?.Invoke();
        if ((serializeFlags & PositionMask) != 0) outFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & AmmoCountMask) != 0) outFacing.OnAmmoCountChanged?.Invoke();
        if ((serializeFlags & FireCooldownMask) != 0) outFacing.OnFireCooldownChanged?.Invoke();
        if ((serializeFlags & ManaAmountMask) != 0) outFacing.OnManaAmountChanged?.Invoke();
        if ((serializeFlags & CastCooldownMask) != 0) outFacing.OnCastCooldownChanged?.Invoke();

        // ReSharper restore EnforceIfStatementBraces

    }

    public TypeInformation TypeInformation
    {
        get
        {
            return new TypeInformation(new TypeInformationField[]
            {
                new() { mask = FireButtonIsDownMask, name = new (nameof(current.fireButtonIsDown)), type = typeof(System.Boolean) },
                new() { mask = CastButtonIsDownMask, name = new (nameof(current.castButtonIsDown)), type = typeof(System.Boolean) },
                new() { mask = AimingMask, name = new (nameof(current.aiming)), type = typeof(Piot.Surge.Types.Aiming) },
                new() { mask = PositionMask, name = new (nameof(current.position)), type = typeof(Piot.Surge.Types.Position3) },
                new() { mask = AmmoCountMask, name = new (nameof(current.ammoCount)), type = typeof(System.UInt16) },
                new() { mask = FireCooldownMask, name = new (nameof(current.fireCooldown)), type = typeof(System.UInt16) },
                new() { mask = ManaAmountMask, name = new (nameof(current.manaAmount)), type = typeof(System.UInt16) },
                new() { mask = CastCooldownMask, name = new (nameof(current.castCooldown)), type = typeof(System.UInt16) },

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
    public EntityRollMode RollMode => internalEntity.RollMode;

    private readonly FireballLogicEntityInternal internalEntity;
    internal FireballLogicEntity(FireballLogicEntityInternal internalEntity)
    {
        this.internalEntity = internalEntity;
    }

    public Tests.ExampleGame.FireballLogic Self => internalEntity.Self;

    public Action? OnDestroyed;
    public Action? OnSpawned;

    public Action? OnPostUpdate;
    public Action? OnPositionChanged;

    public Action? OnVelocityChanged;

    public delegate void ExplodeDelegate();
    public ExplodeDelegate? DoExplode;
    public ExplodeDelegate? UnDoExplode;


}


public class FireballLogicEntityInternal : IGeneratedEntity
{
    private readonly ActionsContainer actionsContainer = new();


public FireballLogicEntityInternal()
    {
        outFacing = new(this);
    }
    Tests.ExampleGame.FireballLogic current;
    Tests.ExampleGame.FireballLogic last;

    public Tests.ExampleGame.FireballLogic Self => current;

        public EntityRollMode RollMode { get; set; }

 internal Tests.ExampleGame.FireballLogic Current
            {
                set => current = value;
            }
     FireballLogicEntity outFacing;
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

        // ReSharper disable EnforceIfStatementBraces

        if (current.position != last.position) mask |= PositionMask;
        if (current.velocity != last.velocity) mask |= VelocityMask;

        return mask;
        // ReSharper restore EnforceIfStatementBraces

    }
    public void FireChanges(ulong serializeFlags)
    {
        // ReSharper disable EnforceIfStatementBraces

        if ((serializeFlags & PositionMask) != 0) outFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & VelocityMask) != 0) outFacing.OnVelocityChanged?.Invoke();

        // ReSharper restore EnforceIfStatementBraces

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
