/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.Entities;
using Piot.Surge.Internal.Generated;
using Piot.Surge.Replay;
using Piot.Surge.Tick;
using Tests.ExampleGame;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class ReplayRecorderTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public ReplayRecorderTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    private static DeltaSnapshotPack TickHost(IEntityContainerWithDetectChanges authoritativeWorld, TickId hostTickId)
    {
        OverWriter.Overwrite(authoritativeWorld);
        Ticker.Tick(authoritativeWorld);
        Notifier.Notify(authoritativeWorld.AllEntities);
        var deltaSnapshotEntityIds = Scanner.Scan(authoritativeWorld, hostTickId);
        return DeltaSnapshotToBitPack.ToDeltaSnapshotPack(authoritativeWorld, deltaSnapshotEntityIds);
    }

    [Fact]
    public void WriteAndReadReplay()
    {
        IEntity spawnedAvatarEntityOnHost;
        AvatarLogicEntityInternal spawnedAvatarInternalOnHost;

        var applicationVersion = new SemanticVersion(1, 2, 3, "-testing");

        {
            var authoritative = new AuthoritativeWorld();
            var notify = new GeneratedEngineWorld();
            var entitySpawner = new GeneratedEngineSpawner(authoritative, notify);

            using var outputStream = FileStreamCreator.Create("replay.temp");


            (spawnedAvatarEntityOnHost, spawnedAvatarInternalOnHost) = entitySpawner.SpawnAvatarLogic(new AvatarLogic
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


            var replayRecorder = new ReplayRecorder(authoritative, new(32), applicationVersion, outputStream,
                log.SubLog("replayRecorder"));
            TickId hostTickId = new(33);
            var deltaSnapshotPack = TickHost(authoritative, hostTickId);
            replayRecorder.AddPack(deltaSnapshotPack, hostTickId);
            replayRecorder.Close();
        }

        {
            var ghostCreator = new GeneratedEntityGhostCreator();
            var notifyWorld = new GeneratedEngineWorld();
            var clientWorld = new WorldWithGhostCreator(ghostCreator, notifyWorld, false);

            var fileStream = FileStreamCreator.OpenWithSeek("replay.temp");
            var now = new Milliseconds(10);
            var replayPlayback = new ReplayPlayback(clientWorld, now, applicationVersion, fileStream,
                log.SubLog("replayPlayback"));
            var clientAvatar = clientWorld.FetchEntity<AvatarLogicEntityInternal>(spawnedAvatarEntityOnHost.Id);

            var chainLightningCount = 0;
            clientAvatar.OutFacing.DoFireChainLightning += direction => { chainLightningCount++; };

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