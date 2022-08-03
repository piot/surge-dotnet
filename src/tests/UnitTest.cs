/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge;
using Piot.Surge.ChangeMask;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInputSerialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaInternal;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotSerialization;
using Piot.Surge.SnnapshotDeltaPack.Serialization;
using Piot.Surge.Types;
using Piot.Surge.TypeSerialization;
using Surge.SnapshotDeltaPack;
using Tests.ExampleGame;
using Xunit.Abstractions;

namespace Tests;

public class CompareLogicalInputCollections : IEqualityComparer<ICollection<LogicalInput>>
{
    public bool Equals(ICollection<LogicalInput>? x, ICollection<LogicalInput>? y)
    {
        if (x is null || y is null) return false;

        if (x.Count != y.Count) return false;

        for (var i = 0; i < x.Count; ++i)
        {
            var a = x.ToArray()[i];
            var b = y.ToArray()[i];
            if (!a.Equals(b)) return false;
        }

        return true;
    }

    public int GetHashCode(ICollection<LogicalInput> obj)
    {
        return 0;
    }
}

public class UnitTest1
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public UnitTest1(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget);
    }

    [Fact]
    public void Test1()
    {
        var writer = new OctetWriter(23);
        writer.WriteUInt8(42);

        Assert.Equal(42, writer.Octets.Span[0]);
    }


    [Fact]
    public void SerializePosition()
    {
        var pos = new Position3(100, -200, 300);

        var writer = new OctetWriter(23);
        Position3Writer.Write(pos, writer);

        var reader = new OctetReader(writer.Octets);

        var encounteredPosition = Position3Reader.Read(reader);

        Assert.Equal(pos, encounteredPosition);
    }

    [Fact]
    public void SerializeLogicalInput()
    {
        var logicalInputQueue = new LogicalInputQueue();
        logicalInputQueue.AddLogicalInput(new LogicalInput
            { appliedAtSnapshotId = new SnapshotId { frameId = 20 }, payload = new byte[] { 0x0a, 0x0b } });

        var writer = new OctetWriter(23);

        LogicalInputSerialize.Serialize(writer, logicalInputQueue.Collection);


        var reader = new OctetReader(writer.Octets);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Equal(logicalInputQueue.Collection, encounteredLogicalPositions, new CompareLogicalInputCollections());
    }

    [Fact]
    public void SerializeEmptyLogicalInput()
    {
        var logicalInputQueue = new LogicalInputQueue();

        var writer = new OctetWriter(23);

        LogicalInputSerialize.Serialize(writer, logicalInputQueue.Collection);

        Assert.Equal(1, writer.Octets.Length);

        var reader = new OctetReader(writer.Octets);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Equal(logicalInputQueue.Collection, encounteredLogicalPositions, new CompareLogicalInputCollections());

        Assert.Throws<IndexOutOfRangeException>(() => reader.ReadUInt8());
    }

    [Fact]
    public void OrderedDatagrams()
    {
        OrderedDatagramsIn sequence = new();
        {
            OctetWriter writer = new (1);
            writer.WriteUInt8(128);
            OctetReader reader = new (writer.Octets);
            Assert.False(sequence.Read(reader));
        }
        {
            OctetWriter writer = new (1);
            writer.WriteUInt8(129);
            OctetReader reader = new (writer.Octets);
            Assert.False(sequence.Read(reader));
        }
    }


    [Fact]
    public void OrderedDatagramsValid()
    {
        OrderedDatagramsIn sequence = new ();
        {
            OctetWriter writer = new (1);
            writer.WriteUInt8(126);
            OctetReader reader = new (writer.Octets);
            Assert.True(sequence.Read(reader));
        }

        {
            OctetWriter writer = new (1);
            writer.WriteUInt8(127);
            OctetReader reader = new (writer.Octets);
            Assert.True(sequence.Read(reader));
        }

        {
            OctetWriter writer = new (1);
            writer.WriteUInt8(127);
            OctetReader reader = new (writer.Octets);
            Assert.False(sequence.Read(reader));
        }
    }

    [Fact]
    public void OrderedDatagramsWrite()
    {
        OrderedDatagramsOut sequence = new ();
        Assert.Equal(0, sequence.SequenceId);

        for (var i = 0; i < 256; ++i)
        {
            OctetWriter writer = new (1);
            sequence.Write(writer);
            OctetReader reader = new (writer.Octets);
            Assert.Equal(i, reader.ReadUInt8());
        }

        Assert.Equal(0, sequence.SequenceId);
        OctetWriter writer2 = new (1);
        sequence.Write(writer2);
        Assert.Equal(1, sequence.SequenceId);
    }

    [Fact]
    public void Position()
    {
        var pos = Position3.FromFloats(65349.244f, 14049.991f, 999.012f);
        var formattedPosition = pos.ToString();
        Assert.Equal("[pos3 65,349.24, 14,049.99, 999.01]", formattedPosition);
    }

    [Fact]
    public void EntitySerializeAll()
    {
        var someAvatar = new AvatarLogicEntityInternal
            { Current = new AvatarLogic { position = new Position3(100, 200, 300), ammoCount = 1234 } };

        var writer = new OctetWriter(64);
        (someAvatar as IEntitySerializer).SerializeAll(writer);
        Assert.Equal(15, writer.Octets.Length);

        var readAvatar = new AvatarLogicEntityInternal();

        var reader = new OctetReader(writer.Octets);

        (readAvatar as IEntityDeserializer).DeserializeAll(reader);

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
    }

    [Fact]
    public void EntitySerializeMask()
    {
        var someAvatar = new AvatarLogicEntityInternal
            { Current = new AvatarLogic { position = new Position3(100, 200, 300), ammoCount = 1234 } };
        var readAvatar = new AvatarLogicEntityInternal();

        {
            var writer = new OctetWriter(64);
            (someAvatar as IEntitySerializer).SerializeAll(writer);
            Assert.Equal(15, writer.Octets.Length);

            var reader = new OctetReader(writer.Octets);

            (readAvatar as IEntityDeserializer).DeserializeAll(reader);
        }

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);

        someAvatar.Current = someAvatar.Self with { ammoCount = (ushort)(someAvatar.Self.ammoCount + 1) };

        Assert.NotEqual(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
        {
            var writer = new OctetWriter(64);

            (someAvatar as IEntitySerializer).Serialize(AvatarLogicEntityInternal.AmmoCountMask, writer);

            Assert.NotEqual(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);

            var reader = new OctetReader(writer.Octets);
            (readAvatar as IEntityDeserializer).Deserialize(AvatarLogicEntityInternal.AmmoCountMask, reader);
        }

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
    }

    [Fact]
    public void EntitySerializeMaskTest()
    {
        var avatarBefore = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100 }
        };

        var changes = (avatarBefore as IEntityChanges).Changes();
        Assert.Equal(AvatarLogicEntityInternal.AmmoCountMask, changes);
        avatarBefore.Overwrite();

        var changesAfter = (avatarBefore as IEntityChanges).Changes();
        Assert.Equal((ulong)0, changesAfter);
    }

    [Fact]
    public void EntityRollback()
    {
        var avatarBeforeInfo = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100, fireButtonIsDown = false }
        };

        var avatarButtonDown = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100, fireButtonIsDown = true }
        };

        var writerButtonDown = new OctetWriter(100);
        (avatarButtonDown as IEntitySerializer).Serialize(AvatarLogicEntityInternal.FireButtonIsDownMask,
            writerButtonDown);
        Assert.Equal(1, writerButtonDown.Octets.Length);
        Assert.Equal(1, writerButtonDown.Octets.Span[0]);

        var readerButtonDown = new OctetReader(writerButtonDown.Octets);

        var writerButtonHistory = new OctetWriter(100);
        DeserializeWithChangeWriter.DeserializeWithChange(avatarBeforeInfo,
            AvatarLogicEntityInternal.FireButtonIsDownMask,
            readerButtonDown, writerButtonHistory);

        Assert.Equal(1, writerButtonHistory.Octets.Length);
        Assert.Equal(0, writerButtonHistory.Octets.Span[0]);

        var rollbackReader = new OctetReader(writerButtonHistory.Octets);

        (avatarBeforeInfo as IEntityDeserializer).Deserialize(AvatarLogicEntityInternal.FireButtonIsDownMask,
            rollbackReader);

        Assert.False(avatarBeforeInfo.Self.fireButtonIsDown);
    }

    [Fact]
    public void EntityChangesMask()
    {
        var avatar = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100, fireButtonIsDown = false }
        };
        avatar.Overwrite();

        Assert.Equal(0, avatar.Self.position.x);
        Assert.Equal(0u, (avatar as IEntityChanges).Changes());
        (avatar as ISimpleLogic).Tick();

        Assert.Equal(3, avatar.Self.position.x);
        Assert.Equal(AvatarLogicEntityInternal.PositionMask, (avatar as IEntityChanges).Changes());
    }

    [Fact]
    public void TestSnapshotIdRanges()
    {
        var first = new SnapshotIdRange(new SnapshotId(23), new SnapshotId(24));
        var illegalAfter = new SnapshotIdRange(new SnapshotId(26), new SnapshotId(28));
        Assert.False(illegalAfter.IsImmediateFollowing(first));
        var legalAfter = new SnapshotIdRange(new SnapshotId(25), new SnapshotId(31));
        Assert.True(legalAfter.IsImmediateFollowing(first));
    }

    [Fact]
    public void IllegalRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SnapshotIdRange(new SnapshotId(24), new SnapshotId(23)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SnapshotIdRange(new SnapshotId(uint.MaxValue), new SnapshotId(uint.MaxValue)));
    }

    private static void SaveDelta(ISnapshotDeltaPackQueue queue, IUpdatedEntity updatedEntity)
    {
        var snapshotId = new SnapshotId(10);
        var octets = SnapshotDeltaPacker.Pack(Array.Empty<EntityId>(), Array.Empty<IEntity>(),
            new[] { updatedEntity });
        var pack = new SnapshotDeltaPack(snapshotId, octets);
        queue.Enqueue(pack);
    }

    [Fact]
    public void FullRollback()
    {
        var avatarInfo = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100, fireButtonIsDown = false }
        };

        var typeInformation = (avatarInfo as IGeneratedEntity).TypeInformation;

        log.Info("typeInfo {TypeInformation}", typeInformation);

        avatarInfo.Overwrite();

        Assert.Equal(0, avatarInfo.Self.position.x);
        Assert.Equal(0u, (avatarInfo as IEntityChanges).Changes());

        var packetQueue = new SnapshotDeltaPackQueue();

        var world = new World(new GeneratedEntityCreation());

        var spawnedAvatar = world.SpawnEntity(avatarInfo);

        var scanWorld = (IEntityContainerWithChanges)world;
        var allEntities = (world as IEntityContainer).AllEntities;
        Ticker.Tick(allEntities);
        Assert.Equal(3, ((AvatarLogic)spawnedAvatar.Logic).position.x);

        var snapshotDelta = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld));
        world.ClearDelta();
        Assert.Empty(snapshotDelta.deletedIds);
        Assert.Empty(snapshotDelta.updatedEntities);
        Assert.Single(snapshotDelta.createdIds);


        Ticker.Tick(allEntities);

        var snapshotDeltaAfter = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld));
        world.ClearDelta();
        Assert.Empty(snapshotDeltaAfter.deletedIds);
        Assert.Empty(snapshotDeltaAfter.createdIds);
        Assert.Single(snapshotDeltaAfter.updatedEntities);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, snapshotDeltaAfter.updatedEntities[0].changeMask.mask);

        Memory<byte> snapshotDeltaPackPayload;
        {
            var (createdForPacker, updateForPacker) = SnapshotDeltaPackPrepare.Prepare(
                snapshotDeltaAfter.createdIds,
                snapshotDeltaAfter.updatedEntities, world);
            Assert.Empty(createdForPacker);
            Assert.Single(updateForPacker);

            snapshotDeltaPackPayload =
                SnapshotDeltaPacker.Pack(snapshotDeltaAfter.deletedIds, createdForPacker, updateForPacker);
        }

        var firstSnapshotId = new SnapshotId(8);

        var snapshotDeltaPack = new SnapshotDeltaPack(firstSnapshotId, snapshotDeltaPackPayload);
        packetQueue.Enqueue(snapshotDeltaPack);

        Assert.Equal(1, packetQueue.Count);
        Assert.Equal(packetQueue.Peek().snapshotId, firstSnapshotId);
        Assert.Equal(25, packetQueue.Peek().payload.Length);

        Assert.Equal(6, ((AvatarLogic)spawnedAvatar.Logic).position.x);
        Assert.Equal(AvatarLogicEntityInternal.PositionMask,
            ((AvatarLogicEntityInternal)spawnedAvatar.GeneratedEntity as IEntityChanges).Changes());

        var firstPack = packetQueue.Dequeue();

        var (deletedEntities, createdEntities, updatedEntities) =
            SnapshotDeltaUnPacker.UnPack(firstPack.payload, world);

        Assert.Single(updatedEntities);
        Assert.Empty(deletedEntities);
        Assert.Empty(createdEntities);
        Assert.Equal(spawnedAvatar.Id, updatedEntities[0].entity.Id);


        (world as IEntityContainer).DeleteEntity(spawnedAvatar);

        {
            var snapshotDeltaAfterDelete = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld));
            var (createdForPacker, updateForPacker) = SnapshotDeltaPackPrepare.Prepare(
                snapshotDeltaAfterDelete.createdIds,
                snapshotDeltaAfterDelete.updatedEntities, world);
            Assert.Single(world.Deleted);
            Assert.Empty(world.Created);
            Assert.Empty(createdForPacker);
            Assert.Empty(updateForPacker);
        }
    }

    private (SerializedSnapshotDeltaPackUnionFlattened, EntityId) PrepareThreeServerSnapshotDeltas()
    {
        var avatarInfo = new AvatarLogicEntityInternal
        {
            Current = new AvatarLogic { ammoCount = 100, fireButtonIsDown = false }
        };

        var world = new World(new GeneratedEntityCreation());

        var spawnedAvatar = world.SpawnEntity(avatarInfo);

        var scanWorld = (IEntityContainerWithChanges)world;

        /* FIRST Snapshot */
        var firstDelta = SnapshotDeltaCreator.Scan(scanWorld);
        var firstDeltaConverted = FromSnapshotDeltaInternal.Convert(firstDelta);
        var firstSnapshotId = new SnapshotId(10);
        var firstDeltaPack = SnapshotDeltaPackCreator.Create(firstSnapshotId, world, firstDeltaConverted);
        world.ClearDelta();
        OverWriter.Overwrite(world);

        var internalInfo = firstDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(FullChangeMask.AllFieldChangedMaskBits, internalInfo.changeMask);

        Assert.Single(firstDeltaConverted.createdIds);
        Assert.Empty(firstDeltaConverted.updatedEntities);
        Assert.Empty(firstDeltaConverted.deletedIds);
        Ticker.Tick(world);
        
        var serverSpawnedAvatarForAssert = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        Assert.Equal(3, serverSpawnedAvatarForAssert.Self.position.x);


        
        /* SECOND Snapshot */
        var secondDelta = SnapshotDeltaCreator.Scan(scanWorld);
        var secondDeltaConverted = FromSnapshotDeltaInternal.Convert(secondDelta);
        var secondInternalInfo = secondDelta.FetchEntity(spawnedAvatar.Id);
        var secondSnapshotId = new SnapshotId(11);
        var secondDeltaPack = SnapshotDeltaPackCreator.Create(secondSnapshotId, world, secondDeltaConverted);
        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondInternalInfo.changeMask);
        world.ClearDelta();
        OverWriter.Overwrite(world);

        Assert.Empty(secondDeltaConverted.createdIds);
        Assert.Single(secondDeltaConverted.updatedEntities);
        Assert.Empty(secondDeltaConverted.deletedIds);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondDeltaConverted.updatedEntities[0].changeMask.mask);

        var serverSpawnedAvatar = (AvatarLogicEntityInternal)spawnedAvatar.GeneratedEntity;
        Ticker.Tick(world);

        
        /* THIRD */
        serverSpawnedAvatar.Current = serverSpawnedAvatar.Self with { fireButtonIsDown = true };
        var serverSpawnedAvatarForAssertAtThree = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        var thirdDelta = SnapshotDeltaCreator.Scan(scanWorld);
        var thirdDeltaConverted = FromSnapshotDeltaInternal.Convert(thirdDelta);
        var thirdSnapshotId = new SnapshotId(12);
        var thirdDeltaPack = SnapshotDeltaPackCreator.Create(thirdSnapshotId, world, thirdDeltaConverted);
        
        var thirdInternalInfo = thirdDelta.FetchEntity(spawnedAvatar.Id);

        log.Info("Server fire happens at position", serverSpawnedAvatar.Self.position.x);
        world.ClearDelta();
        OverWriter.Overwrite(world);
        Ticker.Tick(world);
        

        Assert.Equal(9, serverSpawnedAvatarForAssertAtThree.Self.position.x);
        Assert.IsType<FireVolley>((serverSpawnedAvatar as IEntityActions).Actions[0]);

        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask |
            AvatarLogicEntityInternal.FireButtonIsDownMask, 
            thirdDeltaConverted.updatedEntities[0].changeMask.mask);

        /* FOURTH */
        var fourthDelta = SnapshotDeltaCreator.Scan(scanWorld);
        var fourthDeltaConverted = FromSnapshotDeltaInternal.Convert(fourthDelta);
        var fourthSnapshotId = new SnapshotId(13);
        var fourthDeltaPack = SnapshotDeltaPackCreator.Create(fourthSnapshotId, world, fourthDeltaConverted);
        var fourthInternalInfo = fourthDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask | AvatarLogicEntityInternal.AmmoCountMask |
            AvatarLogicEntityInternal.FireCooldownMask,
            fourthInternalInfo.changeMask);
        
     
        
        Assert.Empty(thirdDeltaConverted.createdIds);
        Assert.Single(thirdDeltaConverted.updatedEntities);
        Assert.Empty(thirdDeltaConverted.deletedIds);
 
        var idRange = new SnapshotIdRange(firstSnapshotId, fourthSnapshotId);
        var union = new SerializedSnapshotDeltaPackUnion
        {
            snapshotIdRange = idRange,
            packs = new[]{ firstDeltaPack, secondDeltaPack, thirdDeltaPack, fourthDeltaPack }
        };


        return (SnapshotDeltaUnionPacker.Pack(union), spawnedAvatar.Id);
    }
    [Fact]
    public void BasicUndo()
    {
        var (allSerializedSnapshots, spawnedAvatarId) = PrepareThreeServerSnapshotDeltas();
        var clientWorld = new World(new GeneratedEntityCreation()) as IEntityContainer;

        var undoWriter = new OctetWriter(1200);
        var unionReader = new OctetReader(allSerializedSnapshots.payload);
        var deserializedUnion = SnapshotDeltaUnionReader.Read(unionReader);

        var firstSnapshotId = allSerializedSnapshots.snapshotIdRange.containsFromSnapshotId;
        var lastSnapshotId = allSerializedSnapshots.snapshotIdRange.snapshotId;
        
        Assert.Equal(firstSnapshotId.frameId, deserializedUnion.snapshotIdRange.containsFromSnapshotId.frameId);
        Assert.Equal(lastSnapshotId.frameId, deserializedUnion.snapshotIdRange.snapshotId.frameId);

        var firstPack = deserializedUnion.packs[0];
        var firstSnapshotReader = new OctetReader(firstPack.payload);
        var (_, _, updateEntitiesInFirst) = SnapshotDeltaReader.Read(firstSnapshotReader, clientWorld);
        
        var clientSpawnedEntity = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatarId);
        var clientAvatar = clientWorld.FetchEntity(spawnedAvatarId);
        
        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Ticker.Tick(clientWorld);
        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Notifier.Notify(updateEntitiesInFirst);
        

        clientSpawnedEntity.OutFacing.OnAmmoCountChanged += () =>
        {
            log.Info("ammo count is {AmmoCount}", clientSpawnedEntity.Self.ammoCount);
        };

        clientSpawnedEntity.OutFacing.OnSpawned += () => { log.Info("SPAWNED {Avatar}", clientSpawnedEntity); };

        clientSpawnedEntity.OutFacing.DoFireVolley += position =>
        {
            log.Info("CLIENT DO FIRE {Position}", position);
        };
        
        var allButTheLastPacks = deserializedUnion.packs.Skip(1).Take(deserializedUnion.packs.Length - 3);
        
        foreach (var snapshotDelta in allButTheLastPacks)
        {
            var snapshotReader = new OctetReader(snapshotDelta.payload);
            var (_, _, updateEntities) = SnapshotDeltaReader.Read(snapshotReader, clientWorld);
            Ticker.Tick(clientWorld);
            Notifier.Notify(updateEntities);
            OverWriter.Overwrite(clientWorld);

        }
        

        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Assert.False(clientSpawnedEntity.Self.fireButtonIsDown);
        Assert.Equal(100, clientSpawnedEntity.Self.ammoCount);
        Assert.Equal(6, clientSpawnedEntity.Self.position.x);

        var secondToLastPack = deserializedUnion.packs[deserializedUnion.packs.Length - 2];
        var secondToLastReader = new OctetReader(secondToLastPack.payload);
        var (_, _, secondToLastUpdatedEntities) = SnapshotDeltaReader.Read(secondToLastReader, clientWorld);

        
        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        Ticker.Tick(clientWorld);
        Notifier.Notify(secondToLastUpdatedEntities);
        OverWriter.Overwrite(clientWorld);

        Assert.Equal(30, clientSpawnedEntity.Self.fireCooldown);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        
        var lastPack = deserializedUnion.packs.Last();
        var lastSnapshotReader = new OctetReader(lastPack.payload);
        
        var (deleted, created, clientUpdated) =
            SnapshotDeltaReaderWithUndo.ReadWithUndo(lastSnapshotReader, clientWorld, undoWriter);
        

        Assert.Equal(30, clientSpawnedEntity.Self.fireCooldown);
        Assert.Equal(99, clientSpawnedEntity.Self.ammoCount);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        
        Ticker.Tick(clientWorld);
        Notifier.Notify(clientUpdated);
        OverWriter.Overwrite(clientWorld);

        var undoPack = new SnapshotDeltaPack(firstSnapshotId, undoWriter.Octets);

        Assert.Equal(29, undoWriter.Octets.Length);

        foreach (var clientCreatedEntity in created) clientCreatedEntity.FireCreated();

        Assert.Equal(12, clientSpawnedEntity.Self.position.x);
        Assert.Equal(99, clientSpawnedEntity.Self.ammoCount);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        Assert.Equal(spawnedAvatarId.Value, clientAvatar.Id.Value);
        Assert.Equal(151u, clientAvatar.Id.Value);


        foreach (var notifyEntity in created)
        {
            foreach (var action in notifyEntity.Actions) notifyEntity.DoAction(action);
            notifyEntity.Overwrite();
        }

        var makeSure = clientWorld.FetchEntity(new EntityId(151));
        Assert.NotNull(makeSure);

        Assert.True(clientAvatar.IsAlive);
        var (deletedUnpack, createdUnpack, updatedUnpack) = SnapshotDeltaUnPacker.UnPack(undoPack.payload, clientWorld);

        Assert.Empty(createdUnpack);
        Assert.Single(updatedUnpack);
        Assert.Empty(deletedUnpack);
        Assert.True(clientAvatar.IsAlive);
        Assert.Equal(9, clientSpawnedEntity.Self.position.x);

        /*
        var readBackAgain = new OctetReader(snapshotDeltaPack.payload);
        SnapshotDeltaReader.Read(readBackAgain, clientWorld);

        var clientSpawnedEntityAgain = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);

        Assert.Equal(3, clientSpawnedEntityAgain.Self.position.x);
        Assert.Equal(100, clientSpawnedEntityAgain.Self.ammoCount);
        */
    }
}
