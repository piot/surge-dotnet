/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge;
using Piot.Surge.ChangeMask;
using Piot.Surge.DatagramType;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInputSerialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDelta;
using Piot.Surge.SnapshotDeltaInternal;
using Piot.Surge.SnapshotDeltaPack;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotSerialization;
using Piot.Surge.SnnapshotDeltaPack.Serialization;
using Piot.Surge.Types;
using Piot.Surge.TypeSerialization;
using Tests.ExampleGame;
using Xunit.Abstractions;

namespace GeneralTests;

public class CompareLogicalInputCollections : IEqualityComparer<ICollection<LogicalInput>>
{
    public bool Equals(ICollection<LogicalInput>? x, ICollection<LogicalInput>? y)
    {
        if (x is null || y is null)
        {
            return false;
        }

        if (x.Count != y.Count)
        {
            return false;
        }

        for (var i = 0; i < x.Count; ++i)
        {
            var a = x.ToArray()[i];
            var b = y.ToArray()[i];
            if (!a.Equals(b))
            {
                return false;
            }
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
    public void OctetWriterFirstOctet()
    {
        var writer = new OctetWriter(23);
        writer.WriteUInt8(42);

        Assert.Equal(42, writer.Octets.Span[0]);
        Assert.Equal(1, writer.Octets.Length);
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
            { appliedAtTickId = new TickId { tickId = 20 }, payload = new byte[] { 0x0a, 0x0b } });

        var writer = new OctetWriter(23);

        LogicalInputSerialize.Serialize(writer, logicalInputQueue.Collection);


        var reader = new OctetReader(writer.Octets);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Equal(logicalInputQueue.Collection, encounteredLogicalPositions, new CompareLogicalInputCollections());
    }

    [Fact]
    public void SerializeLogicalInputDatagramPack()
    {
        var logicalInputQueue = new LogicalInputQueue();
        logicalInputQueue.AddLogicalInput(new LogicalInput
            { appliedAtTickId = new TickId { tickId = 20 }, payload = new byte[] { 0x0a, 0x0b } });

        var datagramsOut = new OrderedDatagramsOut();
        var outDatagram = LogicInputDatagramPackOut.CreateInputDatagram(datagramsOut, new TickId(42), 0, logicalInputQueue.Collection);

        var reader = new OctetReader(outDatagram.ToArray());
        var datagramsSequenceIn = OrderedDatagramsInReader.Read(reader);
        Assert.Equal(datagramsOut.Value, datagramsSequenceIn.Value);
        var typeOfDatagram = DatagramTypeReader.Read(reader);
        Assert.Equal(DatagramType.PredictedInputs, typeOfDatagram);
        SnapshotReceiveStatusReader.Read(reader, out var tickId, out var droppedFrames);
        Assert.Equal(42u, tickId.tickId);
        Assert.Equal(0, droppedFrames);
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
    public void SnapshotIdRanges()
    {
        var first = new TickIdRange(new TickId(23), new TickId(24));
        var illegalAfter = new TickIdRange(new TickId(26), new TickId(28));
        Assert.False(illegalAfter.IsImmediateFollowing(first));
        var legalAfter = new TickIdRange(new TickId(25), new TickId(31));
        Assert.True(legalAfter.IsImmediateFollowing(first));
    }

    [Fact]
    public void IllegalRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TickIdRange(new TickId(24), new TickId(23)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TickIdRange(new TickId(uint.MaxValue), new TickId(uint.MaxValue)));
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

        var firstTick = new TickId(0);
        var snapshotDelta = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld, firstTick));
        world.ClearDelta();
        Assert.Empty(snapshotDelta.deletedIds);
        Assert.Empty(snapshotDelta.updatedEntities);
        Assert.Single(snapshotDelta.createdIds);


        Ticker.Tick(allEntities);
        var secondTick = new TickId(1);
        var snapshotDeltaAfter = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld, secondTick));
        world.ClearDelta();
        Assert.Empty(snapshotDeltaAfter.deletedIds);
        Assert.Empty(snapshotDeltaAfter.createdIds);
        Assert.Single(snapshotDeltaAfter.updatedEntities);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, snapshotDeltaAfter.updatedEntities[0].changeMask.mask);

        SnapshotDeltaIncludedCorrectionPackMemory snapshotDeltaPackPayload;
        {
            /*
            var (createdForPacker, updateForPacker) = SnapshotDeltaPackPrepare.Prepare(
                snapshotDeltaAfter.createdIds,
                snapshotDeltaAfter.updatedEntities, world);
            Assert.Empty(createdForPacker);
            Assert.Single(updateForPacker);
*/
            var snapshotPackContainer = SnapshotDeltaPackCreator.Create(world, snapshotDeltaAfter);

            var snapshotDeltaMemory = SnapshotPackContainerToMemory.PackWithFilter(snapshotPackContainer, Array.Empty<EntityId>());

            var snapshotDeltaPackPayloadWithout =
                SnapshotDeltaPacker.Pack(snapshotDeltaMemory);

            snapshotDeltaPackPayload = new SnapshotDeltaIncludedCorrectionPackMemory
                { memory = snapshotDeltaPackPayloadWithout.memory };
        }

        var firstTickId = new TickId(8);

        var snapshotDeltaPack = new SnapshotDeltaPack(firstTickId, snapshotDeltaPackPayload);
        
        packetQueue.Enqueue(snapshotDeltaPack);

        Assert.Equal(1, packetQueue.Count);
        Assert.Equal(packetQueue.Peek().tickId, firstTickId);
        Assert.Equal(28, packetQueue.Peek().payload.Length);

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
            var snapshotDeltaAfterDelete = FromSnapshotDeltaInternal.Convert(SnapshotDeltaCreator.Scan(scanWorld, firstTickId));
            var (createdForPacker, updateForPacker) = SnapshotDeltaPackPrepare.Prepare(
                snapshotDeltaAfterDelete.createdIds,
                snapshotDeltaAfterDelete.updatedEntities, world);
            Assert.Single(world.Deleted);
            Assert.Empty(world.Created);
            Assert.Empty(createdForPacker);
            Assert.Empty(updateForPacker);
        }
    }


    private static (SnapshotDeltaInternal, SnapshotDelta, SnapshotDeltaPack) ScanConvertAndCreate(World worldToScan, TickId tickId)
    {
        var deltaSnapshotInternal = SnapshotDeltaCreator.Scan(worldToScan, tickId);
        var convertedDeltaSnapshot = FromSnapshotDeltaInternal.Convert(deltaSnapshotInternal);
        var deltaPackContainer = SnapshotDeltaPackCreator.Create(worldToScan, convertedDeltaSnapshot);
        
        worldToScan.ClearDelta();
        OverWriter.Overwrite(worldToScan);
        
        var snapshotDeltaMemory = SnapshotPackContainerToMemory.PackWithFilter(deltaPackContainer, Array.Empty<EntityId>());

        var withoutCorrectionPackMemory = SnapshotDeltaPacker.Pack(snapshotDeltaMemory);
        var complete = new SnapshotDeltaIncludedCorrectionPackMemory
        {
            memory = withoutCorrectionPackMemory.memory
        };

        var deltaPack = new SnapshotDeltaPack(tickId, complete);
        
        return (deltaSnapshotInternal, convertedDeltaSnapshot, deltaPack);
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
        var firstTickId = new TickId(10);
        var (firstDelta, firstDeltaConverted, firstDeltaPack) = ScanConvertAndCreate(world, firstTickId);

        var internalInfo = firstDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(ChangedFieldsMask.AllFieldChangedMaskBits, internalInfo.changeMask);

        Assert.Single(firstDeltaConverted.createdIds);
        Assert.Empty(firstDeltaConverted.updatedEntities);
        Assert.Empty(firstDeltaConverted.deletedIds);
        
        
        Ticker.Tick(world);

        var serverSpawnedAvatarForAssert = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        Assert.Equal(3, serverSpawnedAvatarForAssert.Self.position.x);


        /* SECOND Snapshot */
        var secondTickId = new TickId(11);
        
        var (secondDelta, secondDeltaConverted, secondDeltaPack) = ScanConvertAndCreate(world, secondTickId);
        
        var secondInternalInfo = secondDelta.FetchEntity(spawnedAvatar.Id);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondInternalInfo.changeMask);

        Assert.Empty(secondDeltaConverted.createdIds);
        Assert.Single(secondDeltaConverted.updatedEntities);
        Assert.Empty(secondDeltaConverted.deletedIds);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondDeltaConverted.updatedEntities[0].changeMask.mask);

        var serverSpawnedAvatar = (AvatarLogicEntityInternal)spawnedAvatar.GeneratedEntity;
        Ticker.Tick(world);


        /* THIRD */
        serverSpawnedAvatar.Current = serverSpawnedAvatar.Self with { fireButtonIsDown = true };
        var serverSpawnedAvatarForAssertAtThree = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        var thirdTickId = new TickId(12);

        var (thirdDelta, thirdDeltaConverted, thirdDeltaPack) = ScanConvertAndCreate(world, thirdTickId);
        
        var thirdInternalInfo = thirdDelta.FetchEntity(spawnedAvatar.Id);

        log.Info("Server fire happens at position", serverSpawnedAvatar.Self.position.x);

        Ticker.Tick(world);


        Assert.Equal(9, serverSpawnedAvatarForAssertAtThree.Self.position.x);
        Assert.IsType<FireVolley>((serverSpawnedAvatar as IEntityActions).Actions[0]);

        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask |
            AvatarLogicEntityInternal.FireButtonIsDownMask,
            thirdDeltaConverted.updatedEntities[0].changeMask.mask);

        /* FOURTH */
        var fourthTickId = new TickId(13);

        var (fourthDelta, fourthDeltaConverted, fourthDeltaPack) = ScanConvertAndCreate(world, fourthTickId);
        
        var fourthInternalInfo = fourthDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask | AvatarLogicEntityInternal.AmmoCountMask |
            AvatarLogicEntityInternal.FireCooldownMask,
            fourthInternalInfo.changeMask);


        Assert.Empty(fourthDeltaConverted.createdIds);
        Assert.Single(fourthDeltaConverted.updatedEntities);
        Assert.Empty(fourthDeltaConverted.deletedIds);

        var idRange = new TickIdRange(firstTickId, fourthTickId);
        var union = new SerializedSnapshotDeltaPackUnion
        {
            tickIdRange = idRange,
            packs = new[] { firstDeltaPack, secondDeltaPack, thirdDeltaPack, fourthDeltaPack }
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

        var firstTickId = allSerializedSnapshots.tickIdRange.containsFromTickId;
        var lastTickId = allSerializedSnapshots.tickIdRange.tickId;

        Assert.Equal(firstTickId.tickId, deserializedUnion.tickIdRange.containsFromTickId.tickId);
        Assert.Equal(lastTickId.tickId, deserializedUnion.tickIdRange.tickId.tickId);

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

        clientSpawnedEntity.OutFacing.DoFireVolley += position => { log.Info("CLIENT DO FIRE {Position}", position); };

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

        var undoPack = new SnapshotDeltaPack(firstTickId, new SnapshotDeltaIncludedCorrectionPackMemory{ memory = undoWriter.Octets });

        Assert.Equal(32, undoWriter.Octets.Length);

        foreach (var clientCreatedEntity in created)
        {
            clientCreatedEntity.FireCreated();
        }

        Assert.Equal(12, clientSpawnedEntity.Self.position.x);
        Assert.Equal(99, clientSpawnedEntity.Self.ammoCount);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        Assert.Equal(spawnedAvatarId.Value, clientAvatar.Id.Value);
        Assert.Equal(151u, clientAvatar.Id.Value);


        foreach (var notifyEntity in created)
        {
            foreach (var action in notifyEntity.Actions)
            {
                notifyEntity.DoAction(action);
            }

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