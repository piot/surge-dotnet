/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge;
using Piot.Surge.Corrections;
using Piot.Surge.DatagramType;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.DeltaSnapshot;
using Piot.Surge.DeltaSnapshot.Convert;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.FieldMask;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LogicalInput;
using Piot.Surge.LogicalInput.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotProtocol.ReceiveStatus;
using Piot.Surge.Tick;
using Piot.Surge.Types;
using Piot.Surge.Types.Serialization;
using Tests.ExampleGame;
using Xunit.Abstractions;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Tests.Surge;

sealed class CompareLogicalInputCollections : IEqualityComparer<ICollection<LogicalInput>>
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

public sealed class UnitTest1
{
    readonly ILog log;

    public UnitTest1(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });

        log = new Log(combinedLogTarget);
    }

    [Fact]
    public void OctetWriterFirstOctet()
    {
        var writer = new OctetWriter(23);
        writer.WriteUInt8(42);

        Assert.Equal(42, writer.Octets[0]);
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
        logicalInputQueue.AddLogicalInput(new(new(0), new(20),
            new byte[] { 0x0a, 0x0b }));

        var writer = new OctetWriter(23);

        LogicalInputSerialize.Serialize(writer,
            new(new LogicalInputArrayForPlayer[]
                { new(new(0), logicalInputQueue.Collection) }));

        var reader = new OctetReader(writer.Octets);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Equal(logicalInputQueue.Collection,
            encounteredLogicalPositions.inputForEachPlayerInSequence[0].inputs,
            new CompareLogicalInputCollections());
    }

    [Fact]
    public void SerializeLogicalInputDatagramPack()
    {
        var logicalInputQueue = new LogicalInputQueue();
        logicalInputQueue.AddLogicalInput(new(new(0), new(20),
            new byte[] { 0x0a, 0x0b }));

        var now = new TimeMs(0x954299);

        var datagramsOut = new OrderedDatagramsSequenceId();
        var octetWriter = new OctetWriter(1024);
        LogicInputDatagramSerialize.Serialize(octetWriter, datagramsOut, new(42), 0,
            now,
            new(new LogicalInputArrayForPlayer[]
                { new(new(0), logicalInputQueue.Collection) }));

        var reader = new OctetReader(octetWriter.Octets);
        var datagramsSequenceIn = OrderedDatagramsSequenceIdReader.Read(reader);
        Assert.Equal(datagramsOut.Value, datagramsSequenceIn.Value);
        var typeOfDatagram = DatagramTypeReader.Read(reader);
        Assert.Equal(DatagramType.PredictedInputs, typeOfDatagram);
        var monotonicTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
        Assert.Equal(0x4299, monotonicTimeLowerBits.lowerBits);
        SnapshotReceiveStatusReader.Read(reader, out var tickId, out var droppedFrames);
        Assert.Equal(42u, tickId.tickId);
        Assert.Equal(0, droppedFrames);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Equal(logicalInputQueue.Collection,
            encounteredLogicalPositions.inputForEachPlayerInSequence[0].inputs,
            new CompareLogicalInputCollections());
    }


    [Fact]
    public void SerializeEmptyLogicalInput()
    {
        var logicalInputQueue = new LogicalInputQueue();

        var writer = new OctetWriter(23);

        LogicalInputSerialize.Serialize(writer,
            new(new LogicalInputArrayForPlayer[]
                { new(new(0), logicalInputQueue.Collection) }));

        Assert.Equal(2, writer.Octets.Length);

        var reader = new OctetReader(writer.Octets);
        var encounteredLogicalPositions = LogicalInputDeserialize.Deserialize(reader);

        Assert.Empty(encounteredLogicalPositions.inputForEachPlayerInSequence);

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
            { Current = new() { position = new(100, 200, 300), ammoCount = 1234 } };

        var writer = new OctetWriter(64);
        someAvatar.SerializeAll(writer);
        Assert.Equal(30, writer.Octets.Length);

        var readAvatar = new AvatarLogicEntityInternal();

        var reader = new OctetReader(writer.Octets);

        readAvatar.DeserializeAll(reader);

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
    }

    [Fact]
    public void EntitySerializeMask()
    {
        var someAvatar = new AvatarLogicEntityInternal
            { Current = new() { position = new(100, 200, 300), ammoCount = 1234 } };
        var readAvatar = new AvatarLogicEntityInternal();

        {
            var writer = new OctetWriter(64);
            someAvatar.SerializeAll(writer);
            Assert.Equal(30, writer.Octets.Length);

            var reader = new OctetReader(writer.Octets);

            readAvatar.DeserializeAll(reader);
        }

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);

        someAvatar.Current = someAvatar.Self with { ammoCount = (ushort)(someAvatar.Self.ammoCount + 1) };

        Assert.NotEqual(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
        {
            var writer = new OctetWriter(64);

            someAvatar.Serialize(AvatarLogicEntityInternal.AmmoCountMask, writer);

            Assert.NotEqual(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);

            var reader = new OctetReader(writer.Octets);
            readAvatar.Deserialize(reader);
        }

        Assert.Equal(someAvatar.Self.ammoCount, readAvatar.Self.ammoCount);
    }

    [Fact]
    public void EntitySerializeMaskTest()
    {
        var avatarBefore = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100 }
        };

        var changes = avatarBefore.Changes();
        Assert.Equal(AvatarLogicEntityInternal.AmmoCountMask, changes);
        avatarBefore.ClearChanges();

        var changesAfter = avatarBefore.Changes();
        Assert.Equal((ulong)0, changesAfter);
    }

    [Fact]
    public void EntityRollback()
    {
        var avatarBeforeInfo = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100, fireButtonIsDown = false }
        };

        var avatarButtonDown = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100, fireButtonIsDown = true }
        };

        var writerButtonDown = new OctetWriter(100);
        avatarButtonDown.Serialize(AvatarLogicEntityInternal.FireButtonIsDownMask,
            writerButtonDown);
        Assert.Equal(4, writerButtonDown.Octets.Length);
        Assert.Equal(1, writerButtonDown.Octets[2]);

        var readerButtonDown = new OctetReader(writerButtonDown.Octets);

        var writerButtonHistory = new OctetWriter(100);
        DeserializeWithChangeWriter.DeserializeWithChange(avatarBeforeInfo,
            AvatarLogicEntityInternal.FireButtonIsDownMask,
            readerButtonDown, writerButtonHistory);

        Assert.Equal(4, writerButtonHistory.Octets.Length);
        Assert.Equal(1, writerButtonHistory.Octets[2]);

        var rollbackReader = new OctetReader(writerButtonHistory.Octets);

        avatarBeforeInfo.Deserialize(rollbackReader);

        Assert.False(avatarBeforeInfo.Self.fireButtonIsDown);
    }

    [Fact]
    public void EntityChangesMask()
    {
        var avatar = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100, fireButtonIsDown = false }
        };
        avatar.ClearChanges();

        Assert.Equal(0, avatar.Self.position.x);
        Assert.Equal(0u, avatar.Changes());
        avatar.Tick();

        Assert.Equal(300, avatar.Self.position.x);
        Assert.Equal(AvatarLogicEntityInternal.PositionMask, avatar.Changes());
    }

    [Fact]
    public void SnapshotIdRanges()
    {
        var first = new TickIdRange(new(23), new(24));
        var illegalAfter = new TickIdRange(new(26), new(28));
        Assert.False(illegalAfter.IsImmediateFollowing(first));
        var legalAfter = new TickIdRange(new(25), new(31));
        Assert.True(legalAfter.IsImmediateFollowing(first));
    }

    [Fact]
    public void IllegalRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new TickIdRange(new(24), new(23)));
        Assert.Throws<ArgumentOutOfRangeException>(static () =>
            new TickIdRange(new(uint.MaxValue), new(uint.MaxValue)));
    }


    [Fact]
    public void FullRollback()
    {
        var avatarInfo = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100, fireButtonIsDown = false }
        };

        var typeInformation = avatarInfo.TypeInformation;

        log.Info("typeInfo {TypeInformation}", typeInformation);

        avatarInfo.ClearChanges();

        Assert.Equal(0, avatarInfo.Self.position.x);
        Assert.Equal(0u, avatarInfo.Changes());

        var packetQueue = new SnapshotDeltaPackIncludingCorrectionsQueue();
        var notifyWorld = new GeneratedNotifyEntityCreation();

        var world = new WorldWithGhostCreator(new GeneratedEntityGhostCreator(), notifyWorld, notifyWorld, false);

        var spawnedAvatar = world.SpawnEntity(avatarInfo);

        var scanWorld = (IEntityContainerWithDetectChanges)world;
        var allEntities = (world as IEntityContainer).AllEntities;
        Ticker.Tick(allEntities);
        Assert.Equal(300, ((AvatarLogic)spawnedAvatar.Logic).position.x);

        var firstTick = new TickId(0);
        var snapshotDelta = Scanner.Scan(scanWorld, firstTick);
        world.ClearDelta();
        Assert.Empty(snapshotDelta.deletedIds);
        Assert.Empty(snapshotDelta.updatedEntities);
        Assert.Single(snapshotDelta.createdIds);


        Ticker.Tick(allEntities);
        var secondTick = new TickId(1);
        var deltaSnapshotEntityIdsAfter = Scanner.Scan(scanWorld, secondTick);
        world.ClearDelta();
        Assert.Empty(deltaSnapshotEntityIdsAfter.deletedIds);
        Assert.Empty(deltaSnapshotEntityIdsAfter.createdIds);
        Assert.Single(deltaSnapshotEntityIdsAfter.updatedEntities);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask,
            deltaSnapshotEntityIdsAfter.updatedEntities[0].changeMask.mask);


        {
            /*
            var (createdForPacker, updateForPacker) = SnapshotDeltaPackPrepare.Prepare(
                snapshotDeltaAfter.createdIds,
                snapshotDeltaAfter.updatedEntities, world);
            Assert.Empty(createdForPacker);
            Assert.Single(updateForPacker);
*/
        }
        var writer = new OctetWriter(1024);
        var snapshotDeltaPack =
            DeltaSnapshotToPack.ToDeltaSnapshotPack(world, deltaSnapshotEntityIdsAfter, writer);


        var firstTickId = new TickId(8);

        var fakeCorrectionsWriter = new OctetWriter(120);

        fakeCorrectionsWriter.WriteUInt8(0);


        var fakeIncludingCorrections =
            new SnapshotDeltaPackIncludingCorrections(TickIdRange.FromTickId(firstTickId),
                snapshotDeltaPack.payload.Span, fakeCorrectionsWriter.Octets, SnapshotStreamType.OctetStream,
                SnapshotType.DeltaSnapshot);


        packetQueue.Enqueue(fakeIncludingCorrections);

        Assert.Equal(1, packetQueue.Count);
        Assert.Equal(packetQueue.Peek().Pack.tickIdRange.Last, firstTickId);

#if DEBUG
        Assert.Equal(26, packetQueue.Peek().Pack.deltaSnapshotPackPayload.Length);
#else
        Assert.Equal(23, packetQueue.Peek().Pack.deltaSnapshotPackPayload.Length);
#endif

        Assert.Equal(600, ((AvatarLogic)spawnedAvatar.Logic).position.x);
        Assert.Equal(AvatarLogicEntityInternal.PositionMask,
            ((AvatarLogicEntityInternal)spawnedAvatar.CompleteEntity).Changes());

        var firstPack = packetQueue.Dequeue();

        var (deletedEntities, createdEntities, updatedEntities) =
            SnapshotDeltaUnPacker.UnPack(firstPack.Pack.deltaSnapshotPackPayload.Span, world);

        Assert.Single(updatedEntities);
        Assert.Empty(deletedEntities);
        Assert.Empty(createdEntities);
        Assert.Equal(spawnedAvatar.Id, updatedEntities[0].entity.Id);


        (world as IEntityContainer).DeleteEntity(spawnedAvatar);
    }


    static (EntityMasks, DeltaSnapshotEntityIds, DeltaSnapshotPack) ScanConvertAndCreate(
        AuthoritativeWorld worldToScan,
        TickId tickId, ILog log)
    {
        var deltaSnapshotEntityIds = Scanner.Scan(worldToScan, tickId);
        var entityMasks = DeltaSnapshotToEntityMasks.ToEntityMasks(deltaSnapshotEntityIds);
        var deltaPack =
            DeltaSnapshotToPack.ToDeltaSnapshotPack(worldToScan, deltaSnapshotEntityIds,
                new OctetWriter(Constants.MaxSnapshotOctetSize));

        ChangeClearer.OverwriteAuthoritative(worldToScan);

        return (entityMasks, deltaSnapshotEntityIds, deltaPack);
    }

    (DeltaSnapshotPack[], EntityId) PrepareThreeServerSnapshotDeltas()
    {
        var avatarInfo = new AvatarLogicEntityInternal
        {
            Current = new() { ammoCount = 100, fireButtonIsDown = false }
        };

        var world = new AuthoritativeWorld();

        var spawnedAvatar = world.SpawnEntity(avatarInfo);

        var scanWorld = (IEntityContainerWithDetectChanges)world;

        /* FIRST Snapshot */
        var firstTickId = new TickId(10);
        var (firstDelta, firstDeltaConverted, firstDeltaPack) = ScanConvertAndCreate(world, firstTickId, log);

        var internalInfo = firstDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(ChangedFieldsMask.AllFieldChangedMaskBits, internalInfo);

        Assert.Single(firstDeltaConverted.createdIds);
        Assert.Empty(firstDeltaConverted.updatedEntities);
        Assert.Empty(firstDeltaConverted.deletedIds);


        Ticker.Tick(world);

        var serverSpawnedAvatarForAssert = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        Assert.Equal(300, serverSpawnedAvatarForAssert.Self.position.x);


        /* SECOND Snapshot */
        var secondTickId = new TickId(11);

        var (secondDelta, secondDeltaConverted, secondDeltaPack) = ScanConvertAndCreate(world, secondTickId, log);

        var secondInternalInfo = secondDelta.FetchEntity(spawnedAvatar.Id);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondInternalInfo);

        Assert.Empty(secondDeltaConverted.createdIds);
        Assert.Single(secondDeltaConverted.updatedEntities);
        Assert.Empty(secondDeltaConverted.deletedIds);

        Assert.Equal(AvatarLogicEntityInternal.PositionMask, secondDeltaConverted.updatedEntities[0].changeMask.mask);

        var serverSpawnedAvatar = (AvatarLogicEntityInternal)spawnedAvatar.CompleteEntity;
        Ticker.Tick(world);


        /* THIRD */
        serverSpawnedAvatar.Current = serverSpawnedAvatar.Self with { fireButtonIsDown = true };
        var serverSpawnedAvatarForAssertAtThree = world.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.Id);
        var thirdTickId = new TickId(12);

        var (thirdDelta, thirdDeltaConverted, thirdDeltaPack) = ScanConvertAndCreate(world, thirdTickId, log);

        log.Info("Server fire happens at position", serverSpawnedAvatar.Self.position.x);

        Ticker.Tick(world);


        Assert.Equal(900, serverSpawnedAvatarForAssertAtThree.Self.position.x);
        Assert.IsType<FireChainLightning>(serverSpawnedAvatar.Actions[0]);

        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask |
            AvatarLogicEntityInternal.FireButtonIsDownMask,
            thirdDeltaConverted.updatedEntities[0].changeMask.mask);

        /* FOURTH */
        var fourthTickId = new TickId(13);

        var (fourthDelta, fourthDeltaConverted, fourthDeltaPack) = ScanConvertAndCreate(world, fourthTickId, log);

        var fourthInternalInfo = fourthDelta.FetchEntity(spawnedAvatar.Id);
        Assert.Equal(
            AvatarLogicEntityInternal.PositionMask | AvatarLogicEntityInternal.AmmoCountMask |
            AvatarLogicEntityInternal.FireCooldownMask,
            fourthInternalInfo);


        Assert.Empty(fourthDeltaConverted.createdIds);
        Assert.Single(fourthDeltaConverted.updatedEntities);
        Assert.Empty(fourthDeltaConverted.deletedIds);

        var packs = new[] { firstDeltaPack, secondDeltaPack, thirdDeltaPack, fourthDeltaPack };

        return (packs, spawnedAvatar.Id);
    }

    [Fact]
    public void BasicUndo()
    {
        var (allSerializedSnapshots, spawnedAvatarId) = PrepareThreeServerSnapshotDeltas();
        var notifyWorld = new GeneratedNotifyEntityCreation();
        var clientWorld =
            new WorldWithGhostCreator(new GeneratedEntityGhostCreator(), notifyWorld, notifyWorld, false) as
                IEntityContainerWithGhostCreator;

        var undoWriter = new OctetWriter(1200);

        var firstTickId = allSerializedSnapshots.First().tickIdRange.Last;
        var lastTickId = allSerializedSnapshots.Last().tickIdRange.Last;

        Assert.Equal(firstTickId.tickId, allSerializedSnapshots[0].tickIdRange.startTickId.tickId);
        Assert.Equal(lastTickId.tickId, allSerializedSnapshots[^1].tickIdRange.lastTickId.tickId);

        var firstPack = allSerializedSnapshots[0];
        var firstSnapshotReader = new OctetReader(firstPack.payload.Span);
        var (_, _, updateEntitiesInFirst) = SnapshotDeltaReader.Read(firstSnapshotReader, clientWorld);

        var clientSpawnedEntity = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatarId);
        var clientAvatar = clientWorld.FetchEntity(spawnedAvatarId);

        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Ticker.Tick(clientWorld);
        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Notifier.Notify(updateEntitiesInFirst);

        notifyWorld.OnSpawnAvatarLogic += (_, _) => { log.Info("SPAWNED {Avatar}", clientSpawnedEntity); };

        clientSpawnedEntity.OutFacing.OnAmmoCountChanged += () =>
        {
            log.Info("ammo count is {AmmoCount}", clientSpawnedEntity.Self.ammoCount);
        };


        clientSpawnedEntity.OutFacing.DoFireChainLightning += position =>
        {
            log.Info("CLIENT DO FIRE {Position}", position);
        };

        var allButTheLastPacks = allSerializedSnapshots.Skip(1).Take(allSerializedSnapshots.Length - 3);

        foreach (var snapshotDelta in allButTheLastPacks)
        {
            var snapshotReader = new OctetReader(snapshotDelta.payload.Span);
            var (_, _, updateEntities) = SnapshotDeltaReader.Read(snapshotReader, clientWorld);
            Ticker.Tick(clientWorld);
            Notifier.Notify(updateEntities);
            ChangeClearer.ClearChanges(clientWorld);
        }


        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Assert.False(clientSpawnedEntity.Self.fireButtonIsDown);
        Assert.Equal(100, clientSpawnedEntity.Self.ammoCount);
        Assert.Equal(600, clientSpawnedEntity.Self.position.x);

        var secondToLastPack = allSerializedSnapshots[^2];
        var secondToLastReader = new OctetReader(secondToLastPack.payload.Span);
        var (_, _, secondToLastUpdatedEntities) = SnapshotDeltaReader.Read(secondToLastReader, clientWorld);


        Assert.Equal(0, clientSpawnedEntity.Self.fireCooldown);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        Ticker.Tick(clientWorld);
        Notifier.Notify(secondToLastUpdatedEntities);
        ChangeClearer.ClearChanges(clientWorld);

        Assert.Equal(30, clientSpawnedEntity.Self.fireCooldown);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);

        var lastPack = allSerializedSnapshots.Last();
        var lastSnapshotReader = new OctetReader(lastPack.payload.Span);

        var (deleted, created, clientUpdated) =
            SnapshotDeltaReaderWithUndo.ReadWithUndo(lastSnapshotReader, clientWorld, undoWriter);


        Assert.Equal(30, clientSpawnedEntity.Self.fireCooldown);
        Assert.Equal(99, clientSpawnedEntity.Self.ammoCount);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);

        Ticker.Tick(clientWorld);
        Notifier.Notify(clientUpdated);
        ChangeClearer.ClearChanges(clientWorld);

        var undoPack = new DeltaSnapshotPack(TickIdRange.FromTickId(firstTickId), undoWriter.Octets.ToArray(),
            SnapshotStreamType.OctetStream, SnapshotType.DeltaSnapshot);

#if DEBUG
        Assert.Equal(32, undoWriter.Octets.Length);
#else
        Assert.Equal(29, undoWriter.Octets.Length);
#endif

        Assert.Equal(1200, clientSpawnedEntity.Self.position.x);
        Assert.Equal(99, clientSpawnedEntity.Self.ammoCount);
        Assert.True(clientSpawnedEntity.Self.fireButtonIsDown);
        Assert.Equal(spawnedAvatarId.Value, clientAvatar.Id.Value);
        Assert.Equal(151u, clientAvatar.Id.Value);


        foreach (var notifyEntity in created)
        {
            foreach (var action in notifyEntity.CompleteEntity.Actions)
            {
                notifyEntity.CompleteEntity.DoAction(action);
            }

            notifyEntity.CompleteEntity.ClearChanges();
        }

        var makeSure = clientWorld.FetchEntity(new(151));
        Assert.NotNull(makeSure);

        Assert.True(clientAvatar.IsAlive);
        var (deletedUnpack, createdUnpack, updatedUnpack) =
            SnapshotDeltaUnPacker.UnPack(undoPack.payload.Span, clientWorld);

        Assert.Empty(createdUnpack);
        Assert.Single(updatedUnpack);
        Assert.Empty(deletedUnpack);
        Assert.True(clientAvatar.IsAlive);
        Assert.Equal(900, clientSpawnedEntity.Self.position.x);

        /*
        var readBackAgain = new OctetReader(snapshotDeltaPack.deltaSnapshotPackPayload);
        SnapshotDeltaReader.ReadAndApply(readBackAgain, clientWorld);

        var clientSpawnedEntityAgain = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatar.ArchetypeId);

        Assert.Equal(3, clientSpawnedEntityAgain.Self.position.x);
        Assert.Equal(100, clientSpawnedEntityAgain.Self.ammoCount);
        */
    }
}