/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

// Code generated by Surge generator. DO NOT EDIT.
// <auto-generated /> This file has been auto generated.

#nullable enable


using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.FastTypeInformation;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicAction;
using Piot.Surge.LogicalInput;
using Piot.Surge.Types;
using Piot.Surge.Types.Serialization;
using Tests.ExampleGame;
using Tests.Surge.ExampleGame;

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
    public Action? OnReset;
    public Action<IEntity, AvatarLogicEntity>? OnSpawnAvatarLogic;
    public Action<IEntity, FireballLogicEntity>? OnSpawnFireballLogic;

    public void NotifyGameEngineResetNetworkEntities()
    {
        OnReset?.Invoke();
    }

    void INotifyEntityCreation.CreateGameEngineEntity(IEntity entity)
    {
        switch (entity.CompleteEntity)
        {
            case AvatarLogicEntityInternal internalEntity:
                OnSpawnAvatarLogic?.Invoke(entity, internalEntity.OutFacing);
                break;
            case FireballLogicEntityInternal internalEntity:
                OnSpawnFireballLogic?.Invoke(entity, internalEntity.OutFacing);
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

    public (IEntity, AvatarLogicEntityInternal) SpawnAvatarLogic(AvatarLogic logic)
    {
        var internalEntity = new AvatarLogicEntityInternal
        {
            Current = logic
        };
        var entity = container.SpawnEntity(internalEntity);
        notifyWorld.CreateGameEngineEntity(entity);
        return (entity, internalEntity);
    }

    public (IEntity, FireballLogicEntityInternal) SpawnFireballLogic(FireballLogic logic)
    {
        var internalEntity = new FireballLogicEntityInternal
        {
            Current = logic
        };
        var entity = container.SpawnEntity(internalEntity);
        notifyWorld.CreateGameEngineEntity(entity);
        return (entity, internalEntity);
    }
}

public static class GameInputReader
{
    public static GameInput Read(IOctetReader reader)
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
    public static void Write(IOctetWriter writer, GameInput input)
    {
        AimingWriter.Write(input.aiming, writer);
        writer.WriteUInt8(input.primaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.secondaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.tertiaryAbility ? (byte)1 : (byte)0);
        writer.WriteUInt8(input.ultimateAbility ? (byte)1 : (byte)0);
        Velocity2Writer.Write(input.desiredMovement, writer);
    }
}

public class GeneratedInputPackFetch : IInputPackFetch
{
    InputPackFetch<GameInput>? inputFetcher;

    public Func<LocalPlayerIndex, GameInput> GameSpecificInputFetch
    {
        set => inputFetcher = new(value, GameInputWriter.Write);
    }

    public ReadOnlySpan<byte> Fetch(LocalPlayerIndex index)
    {
        return inputFetcher is null ? ReadOnlySpan<byte>.Empty : inputFetcher.Fetch(index);
    }
}

public static class EventArchetypeConstants
{
    public const byte Explode = 1;
}

public sealed class GeneratedEventEnqueue : IShortEvents
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
    IShortEvents target;

    public GeneratedEventProcessor(IShortEvents target)
    {
        this.target = target;
    }

    public IShortEvents Target
    {
        set => target = value;
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
    public UnitVector3 direction;
}

public struct CastFireball : IAction
{
    public Position3 position;
    public UnitVector3 direction;
}

// --------------- Internal Action Implementation ---------------
public sealed class AvatarLogicActions : AvatarLogic.IAvatarLogicActions
{
    readonly IActionsContainer actionsContainer;

    public AvatarLogicActions(IActionsContainer actionsContainer)
    {
        this.actionsContainer = actionsContainer;
    }

    public void FireChainLightning(UnitVector3 direction)
    {
        actionsContainer.Add(new FireChainLightning { direction = direction });
    }

    public void CastFireball(Position3 position, UnitVector3 direction)
    {
        actionsContainer.Add(new CastFireball { position = position, direction = direction });
    }
}

public sealed class AvatarLogicEntity
{
    public delegate void CastFireballDelegate(Position3 position, UnitVector3 direction);

    public delegate void FireChainLightningDelegate(UnitVector3 direction);

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
    public Action? OnReplicated;

    public Action? OnTestEnumChanged;
    public CastFireballDelegate? UnDoCastFireball;
    public FireChainLightningDelegate? UnDoFireChainLightning;

    internal AvatarLogicEntity(AvatarLogicEntityInternal internalEntity)
    {
        Internal = internalEntity;
    }

    public EntityRollMode RollMode => Internal.RollMode;

    public AvatarLogicEntityInternal Internal { get; }

    public AvatarLogic Self => Internal.Self;

    public override string ToString()
    {
        return $"[AvatarLogicEntity logic:{Self}]";
    }

    public void Destroy()
    {
        Internal.Destroy();
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
    public const ulong TestEnumMask = 0x00000200;

    readonly ActionsContainer actionsContainer = new();
    AvatarLogic current;
    AvatarLogic last;


    public AvatarLogicEntityInternal()
    {
        OutFacing = new(this);
    }

    public AvatarLogic Self => current;


    internal AvatarLogic Current
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
    }

    public void ClearActions()
    {
        actionsContainer.Clear();
    }

    public void FireDestroyed()
    {
        OutFacing.OnDestroyed?.Invoke();
    }

    public void FireReplicate()
    {
        OutFacing.OnReplicated?.Invoke();
    }

    void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
    {
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
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeMarker);
#endif

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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            writer.WriteUInt8((byte)current.testEnum);
        }
    }

    public void Serialize(ulong serializeFlags, IBitWriter writer)
    {
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeMarker);
#endif

        writer.WriteBits((uint)serializeFlags, 10);
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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            writer.WriteBits((uint)current.testEnum, 3);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IOctetWriter writer)
    {
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeMarker);
#endif

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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            writer.WriteUInt8((byte)last.testEnum);
        }
    }

    public void SerializePrevious(ulong serializeFlags, IBitWriter writer)
    {
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeMarker);
#endif

        writer.WriteBits((uint)serializeFlags, 10);
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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            writer.WriteBits((uint)last.testEnum, 3);
        }
    }

    public void SerializeAll(IOctetWriter writer)
    {
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeAllMarker);
#endif
        writer.WriteUInt8(current.fireButtonIsDown ? (byte)1 : (byte)0);
        writer.WriteUInt8(current.castButtonIsDown ? (byte)1 : (byte)0);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteUInt16(current.ammoCount);
        writer.WriteUInt16(current.fireCooldown);
        writer.WriteUInt16(current.manaAmount);
        writer.WriteUInt16(current.castCooldown);
        writer.WriteUInt16(current.jumpTime);
        writer.WriteUInt8((byte)current.testEnum);
    }

    public void SerializeAll(IBitWriter writer)
    {
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeAllMarker);
#endif

        writer.WriteBits(current.fireButtonIsDown ? 1U : 0U, 1);
        writer.WriteBits(current.castButtonIsDown ? 1U : 0U, 1);
        AimingWriter.Write(current.aiming, writer);
        Position3Writer.Write(current.position, writer);
        writer.WriteBits(current.ammoCount, 16);
        writer.WriteBits(current.fireCooldown, 16);
        writer.WriteBits(current.manaAmount, 16);
        writer.WriteBits(current.castCooldown, 16);
        writer.WriteBits(current.jumpTime, 16);
        writer.WriteBits((uint)current.testEnum, 3);
    }

    public void SerializePreviousAll(IOctetWriter writer)
    {
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeAllMarker);
#endif
        writer.WriteUInt8(last.fireButtonIsDown ? (byte)1 : (byte)0);
        writer.WriteUInt8(last.castButtonIsDown ? (byte)1 : (byte)0);
        AimingWriter.Write(last.aiming, writer);
        Position3Writer.Write(last.position, writer);
        writer.WriteUInt16(last.ammoCount);
        writer.WriteUInt16(last.fireCooldown);
        writer.WriteUInt16(last.manaAmount);
        writer.WriteUInt16(last.castCooldown);
        writer.WriteUInt16(last.jumpTime);
        writer.WriteUInt8((byte)last.testEnum);
    }

    public void DeserializeAll(IOctetReader reader)
    {
#if DEBUG
        OctetMarker.AssertMarker(reader, Constants.OctetsSerializeAllMarker);
#endif

        current.fireButtonIsDown = reader.ReadUInt8() != 0;
        current.castButtonIsDown = reader.ReadUInt8() != 0;
        current.aiming = AimingReader.Read(reader);
        current.position = Position3Reader.Read(reader);
        current.ammoCount = reader.ReadUInt16();
        current.fireCooldown = reader.ReadUInt16();
        current.manaAmount = reader.ReadUInt16();
        current.castCooldown = reader.ReadUInt16();
        current.jumpTime = reader.ReadUInt16();
        current.testEnum = (TestEnum)reader.ReadUInt8();
    }

    public void DeserializeAll(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, Constants.BitSerializeAllMarker);
#endif
        current.fireButtonIsDown = reader.ReadBits(1) != 0;
        current.castButtonIsDown = reader.ReadBits(1) != 0;
        current.aiming = AimingReader.Read(reader);
        current.position = Position3Reader.Read(reader);
        current.ammoCount = (ushort)reader.ReadBits(16);
        current.fireCooldown = (ushort)reader.ReadBits(16);
        current.manaAmount = (ushort)reader.ReadBits(16);
        current.castCooldown = (ushort)reader.ReadBits(16);
        current.jumpTime = (ushort)reader.ReadBits(16);
        current.testEnum = (TestEnum)reader.ReadBits(3);
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
#if DEBUG
        OctetMarker.AssertMarker(reader, Constants.OctetsSerializeMarker);
#endif
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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            current.testEnum = (TestEnum)reader.ReadUInt8();
        }

        return serializeFlags;
    }

    public ulong Deserialize(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, Constants.BitSerializeMarker);
#endif

        var serializeFlags = (ulong)reader.ReadBits(10);
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

        if ((serializeFlags & TestEnumMask) != 0)
        {
            current.testEnum = (TestEnum)reader.ReadBits(3);
        }

        return serializeFlags;
    }


    public void Tick()
    {
        var actions = new AvatarLogicActions(actionsContainer);
        current.Tick(actions);
    }


    public void MovementSimulationTick()
    {
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
        if (current.testEnum != last.testEnum) mask |= TestEnumMask;

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
        if ((serializeFlags & TestEnumMask) != 0) OutFacing.OnTestEnumChanged?.Invoke();

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
                new() { mask = JumpTimeMask, name = new(nameof(current.jumpTime)), type = typeof(ushort) },
                new() { mask = TestEnumMask, name = new(nameof(current.testEnum)), type = typeof(TestEnum) }
            });
        }
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

    public ExplodeDelegate? DoExplode;

    public Action? OnDestroyed;
    public Action? OnPositionChanged;
    public Action? OnPostUpdate;
    public Action? OnReplicated;

    public Action? OnVelocityChanged;
    public ExplodeDelegate? UnDoExplode;

    internal FireballLogicEntity(FireballLogicEntityInternal internalEntity)
    {
        Internal = internalEntity;
    }

    public EntityRollMode RollMode => Internal.RollMode;

    public FireballLogicEntityInternal Internal { get; }

    public FireballLogic Self => Internal.Self;

    public override string ToString()
    {
        return $"[FireballLogicEntity logic:{Self}]";
    }

    public void Destroy()
    {
        Internal.Destroy();
    }
}

public sealed class FireballLogicEntityInternal : ICompleteEntity
{
    public const ulong PositionMask = 0x00000001;
    public const ulong VelocityMask = 0x00000002;

    readonly ActionsContainer actionsContainer = new();
    FireballLogic current;
    FireballLogic last;


    public FireballLogicEntityInternal()
    {
        OutFacing = new(this);
    }

    public FireballLogic Self => current;


    internal FireballLogic Current
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
    }

    public void ClearActions()
    {
        actionsContainer.Clear();
    }

    public void FireDestroyed()
    {
        OutFacing.OnDestroyed?.Invoke();
    }

    public void FireReplicate()
    {
        OutFacing.OnReplicated?.Invoke();
    }


    void IAuthoritativeEntityCaptureSnapshot.CaptureSnapshot()
    {
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
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeMarker);
#endif

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
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeMarker);
#endif

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
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeMarker);
#endif

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
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeMarker);
#endif

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
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeAllMarker);
#endif
        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void SerializeAll(IBitWriter writer)
    {
#if DEBUG
        BitMarker.WriteMarker(writer, Constants.BitSerializeAllMarker);
#endif

        Position3Writer.Write(current.position, writer);
        Velocity3Writer.Write(current.velocity, writer);
    }

    public void SerializePreviousAll(IOctetWriter writer)
    {
#if DEBUG
        OctetMarker.WriteMarker(writer, Constants.OctetsSerializeAllMarker);
#endif
        Position3Writer.Write(last.position, writer);
        Velocity3Writer.Write(last.velocity, writer);
    }

    public void DeserializeAll(IOctetReader reader)
    {
#if DEBUG
        OctetMarker.AssertMarker(reader, Constants.OctetsSerializeAllMarker);
#endif

        current.position = Position3Reader.Read(reader);
        current.velocity = Velocity3Reader.Read(reader);
    }

    public void DeserializeAll(IBitReader reader)
    {
#if DEBUG
        BitMarker.AssertMarker(reader, Constants.BitSerializeAllMarker);
#endif
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
#if DEBUG
        OctetMarker.AssertMarker(reader, Constants.OctetsSerializeMarker);
#endif
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
#if DEBUG
        BitMarker.AssertMarker(reader, Constants.BitSerializeMarker);
#endif

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


    public void MovementSimulationTick()
    {
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