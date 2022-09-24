/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

// Code generated by Surge generator. DO NOT EDIT.
// <auto-generated /> This file has been auto generated.

#nullable enable


using System.Numerics;
using Benchmark.Surge.ExampleGame;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.FastTypeInformation;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicAction;
using Piot.Surge.LogicalInput;
using Piot.Surge.Types;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.Internal.Generated;

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

public sealed class GeneratedEntityGhostCreator : IEntityGhostCreator
{
    public IEntity CreateGhostEntity(ArchetypeId archetypeId, EntityId entityId)
    {
        ICompleteEntity completeEntity = archetypeId.id switch
        {
            ArchetypeConstants.AvatarLogic => new AvatarLogicEntityInternal(),
            ArchetypeConstants.FireballLogic => new FireballLogicEntityInternal(),
            _ => throw new($"unknown entity to create {archetypeId}")
        };

        return new Entity(entityId, completeEntity);
    }
}

public sealed class GeneratedNotifyEntityCreation : INotifyEntityCreation, INotifyContainerReset
{
    public Action<AvatarLogicEntity>? OnSpawnAvatarLogic;
    public Action<FireballLogicEntity>? OnSpawnFireballLogic;

    public void NotifyGameEngineResetNetworkEntities()
    {
        throw new NotImplementedException();
    }

    void INotifyEntityCreation.CreateGameEngineEntity(ICompleteEntity entity)
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
                throw new("Internal error");
        }
    }
}

public sealed class GeneratedHostEntitySpawner
{
    readonly IAuthoritativeEntityContainer container;
    readonly INotifyEntityCreation notifyWorld;

    public GeneratedHostEntitySpawner(IAuthoritativeEntityContainer container, INotifyEntityCreation notifyWorld)
    {
        this.container = container;
        this.notifyWorld = notifyWorld;
    }

    public (IEntity, AvatarLogicEntityInternal) SpawnAvatarLogic(BenchmarkAvatarLogic logic)
    {
        var internalEntity = new AvatarLogicEntityInternal
        {
            Current = logic
        };
        notifyWorld.CreateGameEngineEntity(internalEntity);
        return (container.SpawnEntity(internalEntity), internalEntity);
    }

    public (IEntity, FireballLogicEntityInternal) SpawnFireballLogic(BenchmarkFireballLogic logic)
    {
        var internalEntity = new FireballLogicEntityInternal
        {
            Current = logic
        };
        notifyWorld.CreateGameEngineEntity(internalEntity);
        return (container.SpawnEntity(internalEntity), internalEntity);
    }
}

public static class GameInputReader
{
    public static BenchmarkGameInput Read(IOctetReader reader)
    {
        return new()
        {
            aiming = AimingReader.Read(reader),
            primaryAbility = reader.ReadUInt8() != 0,
            secondaryAbility = reader.ReadUInt8() != 0,
            tertiaryAbility = reader.ReadUInt8() != 0,
            ultimateAbility = reader.ReadUInt8() != 0,
            desiredMovement = Velocity2Reader.Read(reader)
        }; // end of new
    }
}

public static class GameInputWriter
{
    public static void Write(IOctetWriter writer, BenchmarkGameInput input)
    {
        AimingWriter.Write(input.aiming, writer);
        writer.WriteUInt8(input.primaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.secondaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.tertiaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.ultimateAbility ? (byte)1 : (byte)0);
        Velocity2Writer.Write(input.desiredMovement, writer);
    }
}

public sealed class GeneratedInputFetch : IInputPackFetch
{
    public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
    {
        var gameInput = GameInputFetch.ReadFromDevice(index); // Found from scanning
        var writer = new OctetWriter(256);
        GameInputWriter.Write(writer, gameInput);

        return writer.Octets;
    }
}

public static class EventArchetypeConstants
{
    public const byte Explode = 1;
}

public sealed class GeneratedEventEnqueue : IBenchmarkShortEvents
{
    readonly EventStreamPackQueue eventStream;

    public GeneratedEventEnqueue(EventStreamPackQueue eventStream)
    {
        this.eventStream = eventStream;
    }

    public void Explode(Position3 position, byte magnitude)

    {
        var writer = eventStream.BitWriter;
#if DEBUG
        BitMarker.WriteMarker(writer, 0x5, 3);
#endif
        writer.WriteBits(EventArchetypeConstants.Explode, 7);

        Position3Writer.Write(position, writer);
        writer.WriteBits(magnitude, 8);
    }
}

public sealed class GeneratedEventProcessor : IEventProcessor
{
    readonly IBenchmarkShortEvents target;

    public GeneratedEventProcessor(IBenchmarkShortEvents target)
    {
        this.target = target;
    }

    public void ReadAndApply(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, 0x5, 3);
#endif
        var archetypeValue = reader.ReadBits(7);

        switch (archetypeValue)
        {
            case EventArchetypeConstants.Explode:
                target.Explode(
                    Position3Reader.Read(reader), (byte)reader.ReadBits(8));
                break;

            default:
                throw new($"Unknown event {archetypeValue}");
        }
    }

    public void SkipOneEvent(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, 0x5, 3);
#endif
        var archetypeValue = reader.ReadBits(7);

        switch (archetypeValue)
        {
            case EventArchetypeConstants.Explode:

                Position3Reader.Read(reader);
                reader.ReadBits(8);

                break;

            default:
                throw new($"Unknown event to skip {archetypeValue}");
        }
    }
}

// --------------- Internal Action Structs ---------------
public struct FireChainLightning : IAction
{
    public Vector3 direction;
}

public struct CastFireball : IAction
{
    public Position3 position;
    public Vector3 direction;
}

// --------------- Internal Action Implementation ---------------
public sealed class AvatarLogicActions : BenchmarkAvatarLogic.IAvatarLogicActions
{
    readonly IActionsContainer actionsContainer;

    public AvatarLogicActions(IActionsContainer actionsContainer)
    {
        this.actionsContainer = actionsContainer;
    }

    public void FireChainLightning(Vector3 direction)
    {
        actionsContainer.Add(new FireChainLightning { direction = direction });
    }

    public void CastFireball(Position3 position, UnitVector3 direction)
    {
    }

    public void CastFireball(Position3 position, Vector3 direction)
    {
        actionsContainer.Add(new CastFireball { position = position, direction = direction });
    }
}

public sealed class AvatarLogicEntity
{
    public delegate void CastFireballDelegate(Position3 position, Vector3 direction);

    public delegate void FireChainLightningDelegate(Vector3 direction);

    readonly AvatarLogicEntityInternal internalEntity;
    public CastFireballDelegate? DoCastFireball;
    public FireChainLightningDelegate? DoFireChainLightning;

    public Action? OnAimingChanged;

    public Action? OnAmmoCountChanged;

    public Action? OnCastButtonIsDownChanged;

    public Action? OnCastCooldownChanged;

    public Action? OnDestroyed;
    public Action? OnFireButtonIsDownChanged;

    public Action? OnFireCooldownChanged;

    public Action? OnJumpTimeChanged;

    public Action? OnManaAmountChanged;

    public Action? OnPositionChanged;
    public Action? OnPostUpdate;
    public CastFireballDelegate? UnDoCastFireball;
    public FireChainLightningDelegate? UnDoFireChainLightning;

    internal AvatarLogicEntity(AvatarLogicEntityInternal internalEntity)
    {
        this.internalEntity = internalEntity;
    }

    public EntityRollMode RollMode => internalEntity.RollMode;

    public BenchmarkAvatarLogic Self => internalEntity.Self;

    public override string ToString()
    {
        return $"[AvatarLogicEntity logic:{Self}]";
    }

    public void Destroy()
    {
        internalEntity.Destroy();
    }
}

public sealed class AvatarLogicEntityInternal : ICompleteEntity, IInputDeserialize
{
    public const ulong FireButtonIsDownMask = 0x00000001;
    public const ulong CastButtonIsDownMask = 0x00000002;
    public const ulong AimingMask = 0x00000004;
    public const ulong PositionMask = 0x00000008;
    public const ulong AmmoCountMask = 0x00000010;
    public const ulong FireCooldownMask = 0x00000020;
    public const ulong ManaAmountMask = 0x00000040;
    public const ulong CastCooldownMask = 0x00000080;
    public const ulong JumpTimeMask = 0x00000100;

    readonly ActionsContainer actionsContainer = new();
    BenchmarkAvatarLogic current;
    BenchmarkAvatarLogic last;


    public AvatarLogicEntityInternal()
    {
        OutFacing = new(this);
    }

    public BenchmarkAvatarLogic Self => current;


    internal BenchmarkAvatarLogic Current
    {
        set => current = value;
    }

    public AvatarLogicEntity OutFacing { get; }

    public EntityRollMode RollMode { get; set; }

    public ArchetypeId ArchetypeId => ArchetypeIdConstants.AvatarLogic;


    public IAction[] Actions => actionsContainer.Actions;

    public ILogic Logic => current;

    public void ClearChanges()
    {
        last = current;
        actionsContainer.Clear();
    }

    public void FireDestroyed()
    {
        OutFacing.OnDestroyed?.Invoke();
    }

    void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
    {
        throw new NotImplementedException();
    }

    public void DoAction(IAction action)
    {
        switch (action)
        {
            case FireChainLightning thing:
                OutFacing.DoFireChainLightning?.Invoke(thing.direction);
                break;
            case CastFireball thing:
                OutFacing.DoCastFireball?.Invoke(thing.position, thing.direction);
                break;
        }
    }

    public void UnDoAction(IAction action)
    {
        switch (action)
        {
            case FireChainLightning thing:
                OutFacing.UnDoFireChainLightning?.Invoke(thing.direction);
                break;
            case CastFireball thing:
                OutFacing.UnDoCastFireball?.Invoke(thing.position, thing.direction);
                break;
        }
    }

    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
        writer.WriteUInt16((ushort)serializeFlags);
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            writer.WriteUInt8(current.castButtonIsDown ? (byte)1 : (byte)0);
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            AimingWriter.Write(current.aiming, writer);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(current.position, writer);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            writer.WriteUInt16(current.ammoCount);
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            writer.WriteUInt16(current.fireCooldown);
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            writer.WriteUInt16(current.manaAmount);
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            writer.WriteUInt16(current.castCooldown);
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            writer.WriteUInt16(current.jumpTime);
        }
    }

    public void Serialize(ulong serializeFlags, IBitWriter writer)
    {
        writer.WriteBits((uint)serializeFlags, 9);
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            writer.WriteBits(current.fireButtonIsDown ? 1U : 0U, 1);
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            writer.WriteBits(current.castButtonIsDown ? 1U : 0U, 1);
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            AimingWriter.Write(current.aiming, writer);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(current.position, writer);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            writer.WriteBits(current.ammoCount, 16);
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            writer.WriteBits(current.fireCooldown, 16);
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            writer.WriteBits(current.manaAmount, 16);
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            writer.WriteBits(current.castCooldown, 16);
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            writer.WriteBits(current.jumpTime, 16);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
        writer.WriteUInt16((ushort)serializeFlags);
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            writer.WriteUInt8(last.fireButtonIsDown ? (byte)1 : (byte)0);
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            writer.WriteUInt8(last.castButtonIsDown ? (byte)1 : (byte)0);
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            AimingWriter.Write(last.aiming, writer);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(last.position, writer);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            writer.WriteUInt16(last.ammoCount);
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            writer.WriteUInt16(last.fireCooldown);
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            writer.WriteUInt16(last.manaAmount);
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            writer.WriteUInt16(last.castCooldown);
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            writer.WriteUInt16(last.jumpTime);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IBitWriter writer)
    {
        writer.WriteBits((uint)serializeFlags, 9);
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            writer.WriteBits(last.fireButtonIsDown ? 1U : 0U, 1);
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            writer.WriteBits(last.castButtonIsDown ? 1U : 0U, 1);
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            AimingWriter.Write(last.aiming, writer);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(last.position, writer);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            writer.WriteBits(last.ammoCount, 16);
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            writer.WriteBits(last.fireCooldown, 16);
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            writer.WriteBits(last.manaAmount, 16);
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            writer.WriteBits(last.castCooldown, 16);
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            writer.WriteBits(last.jumpTime, 16);
        }
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
        writer.WriteUInt16(current.jumpTime);
    }

    public void SerializeAll(IBitWriter writer)
    {
        writer.WriteBits(current.fireButtonIsDown ? 1U : 0U, 1);
        writer.WriteBits(current.castButtonIsDown ? 1U : 0U, 1);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteBits(current.ammoCount, 16);
        writer.WriteBits(current.fireCooldown, 16);
        writer.WriteBits(current.manaAmount, 16);
        writer.WriteBits(current.castCooldown, 16);
        writer.WriteBits(current.jumpTime, 16);
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
        current.jumpTime = reader.ReadUInt16();
    }

    public void DeserializeAll(IBitReader reader)
    {
        current.fireButtonIsDown = reader.ReadBits(1) != 0;
        current.castButtonIsDown = reader.ReadBits(1) != 0;
        current.aiming = AimingReader.Read(reader);
        current.position = Position3Reader.Read(reader);
        current.ammoCount = (ushort)reader.ReadBits(16);
        current.fireCooldown = (ushort)reader.ReadBits(16);
        current.manaAmount = (ushort)reader.ReadBits(16);
        current.castCooldown = (ushort)reader.ReadBits(16);
        current.jumpTime = (ushort)reader.ReadBits(16);
    }

    public void SerializeCorrectionState(IOctetWriter writer)
    {
    }

    public void SerializeCorrectionState(IBitWriter writer)
    {
    }

    public void DeserializeCorrectionState(IOctetReader reader)
    {
    }

    public void DeserializeCorrectionState(IBitReader reader)
    {
    }

    public ulong Deserialize(IOctetReader reader)
    {
        var serializeFlags = (ulong)reader.ReadUInt16();
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            current.fireButtonIsDown = reader.ReadUInt8() != 0;
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            current.castButtonIsDown = reader.ReadUInt8() != 0;
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            current.aiming = AimingReader.Read(reader);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            current.position = Position3Reader.Read(reader);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            current.ammoCount = reader.ReadUInt16();
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            current.fireCooldown = reader.ReadUInt16();
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            current.manaAmount = reader.ReadUInt16();
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            current.castCooldown = reader.ReadUInt16();
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            current.jumpTime = reader.ReadUInt16();
        }

        return serializeFlags;
    }

    public ulong Deserialize(IBitReader reader)
    {
        var serializeFlags = (ulong)reader.ReadBits(9);
        if ((serializeFlags & FireButtonIsDownMask) != 0)
        {
            current.fireButtonIsDown = reader.ReadBits(1) != 0;
        }

        if ((serializeFlags & CastButtonIsDownMask) != 0)
        {
            current.castButtonIsDown = reader.ReadBits(1) != 0;
        }

        if ((serializeFlags & AimingMask) != 0)
        {
            current.aiming = AimingReader.Read(reader);
        }

        if ((serializeFlags & PositionMask) != 0)
        {
            current.position = Position3Reader.Read(reader);
        }

        if ((serializeFlags & AmmoCountMask) != 0)
        {
            current.ammoCount = (ushort)reader.ReadBits(16);
        }

        if ((serializeFlags & FireCooldownMask) != 0)
        {
            current.fireCooldown = (ushort)reader.ReadBits(16);
        }

        if ((serializeFlags & ManaAmountMask) != 0)
        {
            current.manaAmount = (ushort)reader.ReadBits(16);
        }

        if ((serializeFlags & CastCooldownMask) != 0)
        {
            current.castCooldown = (ushort)reader.ReadBits(16);
        }

        if ((serializeFlags & JumpTimeMask) != 0)
        {
            current.jumpTime = (ushort)reader.ReadBits(16);
        }

        return serializeFlags;
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
        if (current.jumpTime != last.jumpTime) mask |= JumpTimeMask;

        return mask;
        // ReSharper restore EnforceIfStatementBraces
    }

    public void FireChanges(ulong serializeFlags)
    {
        // ReSharper disable EnforceIfStatementBraces

        if ((serializeFlags & FireButtonIsDownMask) != 0) OutFacing.OnFireButtonIsDownChanged?.Invoke();
        if ((serializeFlags & CastButtonIsDownMask) != 0) OutFacing.OnCastButtonIsDownChanged?.Invoke();
        if ((serializeFlags & AimingMask) != 0) OutFacing.OnAimingChanged?.Invoke();
        if ((serializeFlags & PositionMask) != 0) OutFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & AmmoCountMask) != 0) OutFacing.OnAmmoCountChanged?.Invoke();
        if ((serializeFlags & FireCooldownMask) != 0) OutFacing.OnFireCooldownChanged?.Invoke();
        if ((serializeFlags & ManaAmountMask) != 0) OutFacing.OnManaAmountChanged?.Invoke();
        if ((serializeFlags & CastCooldownMask) != 0) OutFacing.OnCastCooldownChanged?.Invoke();
        if ((serializeFlags & JumpTimeMask) != 0) OutFacing.OnJumpTimeChanged?.Invoke();

        // ReSharper restore EnforceIfStatementBraces
    }

    public TypeInformation TypeInformation
    {
        get
        {
            return new(new TypeInformationField[]
            {
                new()
                {
                    mask = FireButtonIsDownMask, name = new(nameof(current.fireButtonIsDown)), type = typeof(bool)
                },
                new()
                {
                    mask = CastButtonIsDownMask, name = new(nameof(current.castButtonIsDown)), type = typeof(bool)
                },
                new() { mask = AimingMask, name = new(nameof(current.aiming)), type = typeof(Aiming) },
                new() { mask = PositionMask, name = new(nameof(current.position)), type = typeof(Position3) },
                new() { mask = AmmoCountMask, name = new(nameof(current.ammoCount)), type = typeof(ushort) },
                new() { mask = FireCooldownMask, name = new(nameof(current.fireCooldown)), type = typeof(ushort) },
                new() { mask = ManaAmountMask, name = new(nameof(current.manaAmount)), type = typeof(ushort) },
                new() { mask = CastCooldownMask, name = new(nameof(current.castCooldown)), type = typeof(ushort) },
                new() { mask = JumpTimeMask, name = new(nameof(current.jumpTime)), type = typeof(ushort) }
            });
        }
    }

    public void MovementSimulationTick()
    {
        throw new NotImplementedException();
    }

    public void ClearActions()
    {
        throw new NotImplementedException();
    }


    public void SetInput(IOctetReader reader)
    {
        current.SetInput(GameInputReader.Read(reader));
    }

    public void Destroy()
    {
        // TODO: Add implementation
    }

    public override string ToString()
    {
        return $"[AvatarLogicEntityInternal logic={Self}]";
    }
}

// --------------- Internal Action Structs ---------------
public struct Explode : IAction
{
}

// --------------- Internal Action Implementation ---------------
public sealed class FireballLogicActions : IFireballLogicActions
{
    readonly IActionsContainer actionsContainer;

    public FireballLogicActions(IActionsContainer actionsContainer)
    {
        this.actionsContainer = actionsContainer;
    }

    public void Explode()
    {
        actionsContainer.Add(new Explode());
    }
}

public sealed class FireballLogicEntity
{
    public delegate void ExplodeDelegate();

    readonly FireballLogicEntityInternal internalEntity;
    public ExplodeDelegate? DoExplode;

    public Action? OnDestroyed;
    public Action? OnPositionChanged;
    public Action? OnPostUpdate;

    public Action? OnVelocityChanged;
    public ExplodeDelegate? UnDoExplode;

    internal FireballLogicEntity(FireballLogicEntityInternal internalEntity)
    {
        this.internalEntity = internalEntity;
    }

    public EntityRollMode RollMode => internalEntity.RollMode;

    public BenchmarkFireballLogic Self => internalEntity.Self;

    public override string ToString()
    {
        return $"[FireballLogicEntity logic:{Self}]";
    }

    public void Destroy()
    {
        internalEntity.Destroy();
    }
}

public sealed class FireballLogicEntityInternal : ICompleteEntity
{
    public const ulong PositionMask = 0x00000001;
    public const ulong VelocityMask = 0x00000002;

    readonly ActionsContainer actionsContainer = new();
    BenchmarkFireballLogic current;
    BenchmarkFireballLogic last;


    public FireballLogicEntityInternal()
    {
        OutFacing = new(this);
    }

    public BenchmarkFireballLogic Self => current;


    internal BenchmarkFireballLogic Current
    {
        set => current = value;
    }

    public FireballLogicEntity OutFacing { get; }

    public EntityRollMode RollMode { get; set; }

    public ArchetypeId ArchetypeId => ArchetypeIdConstants.FireballLogic;


    public IAction[] Actions => actionsContainer.Actions;

    public ILogic Logic => current;

    public void ClearChanges()
    {
        last = current;
        actionsContainer.Clear();
    }

    public void FireDestroyed()
    {
        OutFacing.OnDestroyed?.Invoke();
    }


    void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
    {
        throw new NotImplementedException();
    }

    public void DoAction(IAction action)
    {
        switch (action)
        {
            case Explode:
                OutFacing.DoExplode?.Invoke();
                break;
        }
    }

    public void UnDoAction(IAction action)
    {
        switch (action)
        {
            case Explode:
                OutFacing.UnDoExplode?.Invoke();
                break;
        }
    }

    public void Serialize(ulong serializeFlags, IOctetWriter writer)
    {
        writer.WriteUInt8((byte)serializeFlags);
        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(current.position, writer);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            Velocity3Writer.Write(current.velocity, writer);
        }
    }

    public void Serialize(ulong serializeFlags, IBitWriter writer)
    {
        writer.WriteBits((uint)serializeFlags, 2);
        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(current.position, writer);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            Velocity3Writer.Write(current.velocity, writer);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
        writer.WriteUInt8((byte)serializeFlags);
        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(last.position, writer);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            Velocity3Writer.Write(last.velocity, writer);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IBitWriter writer)
    {
        writer.WriteBits((uint)serializeFlags, 2);
        if ((serializeFlags & PositionMask) != 0)
        {
            Position3Writer.Write(last.position, writer);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            Velocity3Writer.Write(last.velocity, writer);
        }
    }

    public void SerializeAll(IOctetWriter writer)
    {
        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void SerializeAll(IBitWriter writer)
    {
        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void DeserializeAll(IOctetReader reader)
    {
        current.position = Position3Reader.Read(reader);
        current.velocity = Velocity3Reader.Read(reader);
    }

    public void DeserializeAll(IBitReader reader)
    {
        current.position = Position3Reader.Read(reader);
        current.velocity = Velocity3Reader.Read(reader);
    }

    public void SerializeCorrectionState(IOctetWriter writer)
    {
    }

    public void SerializeCorrectionState(IBitWriter writer)
    {
    }

    public void DeserializeCorrectionState(IOctetReader reader)
    {
    }

    public void DeserializeCorrectionState(IBitReader reader)
    {
    }

    public ulong Deserialize(IOctetReader reader)
    {
        var serializeFlags = (ulong)reader.ReadUInt8();
        if ((serializeFlags & PositionMask) != 0)
        {
            current.position = Position3Reader.Read(reader);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            current.velocity = Velocity3Reader.Read(reader);
        }

        return serializeFlags;
    }

    public ulong Deserialize(IBitReader reader)
    {
        var serializeFlags = (ulong)reader.ReadBits(2);
        if ((serializeFlags & PositionMask) != 0)
        {
            current.position = Position3Reader.Read(reader);
        }

        if ((serializeFlags & VelocityMask) != 0)
        {
            current.velocity = Velocity3Reader.Read(reader);
        }

        return serializeFlags;
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

        if ((serializeFlags & PositionMask) != 0) OutFacing.OnPositionChanged?.Invoke();
        if ((serializeFlags & VelocityMask) != 0) OutFacing.OnVelocityChanged?.Invoke();

        // ReSharper restore EnforceIfStatementBraces
    }

    public TypeInformation TypeInformation
    {
        get
        {
            return new(new TypeInformationField[]
            {
                new() { mask = PositionMask, name = new(nameof(current.position)), type = typeof(Position3) },
                new() { mask = VelocityMask, name = new(nameof(current.velocity)), type = typeof(Velocity3) }
            });
        }
    }

    public void MovementSimulationTick()
    {
        throw new NotImplementedException();
    }

    public void ClearActions()
    {
        throw new NotImplementedException();
    }

    public void Destroy()
    {
        // TODO: Add implementation
    }

    public override string ToString()
    {
        return $"[FireballLogicEntityInternal logic={Self}]";
    }
}


// Namespace