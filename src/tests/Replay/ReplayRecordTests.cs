/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Raff;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.Entities;
using Piot.Surge.Event;
using Piot.Surge.Internal.Generated;
using Piot.Surge.Replay;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.Tick;
using Tests.Surge.ExampleGame;
using Xunit.Abstractions;

namespace Tests.Replay;

public static class Constants
{
    public static FourCC ReplayName = FourCC.Make("qps1");
    public static FourCC ReplayIcon = new(0xF09F8E9E); // Film frames

    public static FourCC CompleteStateName = FourCC.Make("qst1");
    public static FourCC CompleteStateIcon = new(0xF09F96BC); // Picture Frame

    public static FourCC DeltaStateName = FourCC.Make("qds1");
    public static FourCC DeltaStateIcon = new(0xF09FA096); // Right Arrow


    public static ReplayFileSerializationInfo ReplayInfo = new(
        new(ReplayIcon, ReplayName),
        new(CompleteStateIcon, CompleteStateName),
        new(DeltaStateIcon, DeltaStateName)
    );
}

public sealed class ReplayRecorderTests
{
    readonly ILog log;

    public ReplayRecorderTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    static DeltaSnapshotPack TickHost(IEntityContainerWithDetectChanges authoritativeWorld, TickId hostTickId)
    {
        ChangeClearer.OverwriteAuthoritative(authoritativeWorld);
        Ticker.Tick(authoritativeWorld);
        Notifier.Notify(authoritativeWorld.AllEntities);
        var eventStream = new EventStreamPackQueue(hostTickId);

        var eventEnqueue = new GeneratedEventEnqueue(eventStream);
        if (hostTickId.tickId == 33)
        {
            eventEnqueue.Explode(new(100, 200, 300), 23);
        }

        eventStream.EndOfTick(hostTickId);

        var shortLivedEvents = eventStream.FetchEventsForRange(TickIdRange.FromTickId(hostTickId));

        var bitWriter = new BitWriter(Piot.Surge.SnapshotProtocol.Constants.MaxSnapshotOctetSize);
        var deltaSnapshotEntityIds = Scanner.Scan(authoritativeWorld, hostTickId);
        return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(authoritativeWorld,
            shortLivedEvents, deltaSnapshotEntityIds,
            TickIdRange.FromTickId(hostTickId), bitWriter);
    }

    [Fact]
    public void WriteAndReadReplay()
    {
        IEntity spawnedAvatarEntityOnHost;
        AvatarLogicEntityInternal spawnedAvatarInternalOnHost;

        var applicationVersion = new SemanticVersion(1, 2, 3, "-testing");

        {
            var authoritative = new AuthoritativeWorld();
            var notify = new GeneratedNotifyEntityCreation();
            var entitySpawner = new GeneratedHostEntitySpawner(authoritative, notify);


            using var outputStream = FileStreamCreator.Create("replay.temp");

            (spawnedAvatarEntityOnHost, spawnedAvatarInternalOnHost) = entitySpawner.SpawnAvatarLogic(new()
            {
                fireButtonIsDown = true,
                castButtonIsDown = false,
                aiming = default,
                position = default,
                ammoCount = 10,
                fireCooldown = 0,
                manaAmount = 16,
                castCooldown = 0,
                jumpTime = 0
            });

            var replayRecorder = new ReplayRecorder(authoritative, new(14800), new(32), applicationVersion,
                Constants.ReplayInfo, outputStream,
                log.SubLog("replayRecorder"));
            TickId hostTickId = new(33);
            var deltaSnapshotPack = TickHost(authoritative, hostTickId);
            replayRecorder.AddPack(deltaSnapshotPack, new(14830), hostTickId);
            replayRecorder.Close();
        }

        {
            var ghostCreator = new GeneratedEntityGhostCreator();
            var notifyWorld = new GeneratedNotifyEntityCreation();
            var eventTarget =
                new GeneratedEventProcessor(new ShortEvents(log.SubLog("EventTarget")));
            var clientWorld = new WorldWithGhostCreator(ghostCreator, notifyWorld, false);

            var fileStream = FileStreamCreator.OpenWithSeek("replay.temp");
            var now = new TimeMs(10);
            var replayPlayback = new ReplayPlayback(clientWorld, eventTarget, now, applicationVersion,
                Constants.ReplayInfo, fileStream,
                log.SubLog("replayPlayback"));
            var clientAvatar = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatarEntityOnHost.Id);

            var chainLightningCount = 0;
            clientAvatar.OutFacing.DoFireChainLightning = direction => { chainLightningCount++; };

            Assert.Equal(10, clientAvatar.Self.ammoCount);
            Assert.Equal(0, chainLightningCount);

            for (var i = 30; i < 300; i += 20)
            {
                replayPlayback.Update(new(i));
            }

            log.Info("compare {Expected} {Encountered}", spawnedAvatarInternalOnHost.Self, clientAvatar.Self);
            Assert.Equal(spawnedAvatarInternalOnHost.Self, clientAvatar.Self);
            Assert.Equal(1, chainLightningCount);
        }
    }
}