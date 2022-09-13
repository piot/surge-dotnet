/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Tick;
using Piot.Surge.Types;
using Xunit.Abstractions;

namespace Tests.ExampleGame;

public sealed class Rollforth
{
    private readonly ILog log;

    public Rollforth(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);

        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void PredictRollbackAndRollforthEntity()
    {
        var internalEntity = new AvatarLogicEntityInternal
        {
            Current = new()
            {
                ammoCount = 20
            }
        };

        var positionChangedCount = 0;
        var expectedRollMode = EntityRollMode.Predict;
        var rollbackPositionChangedCount = 0;
        var ammoChangedCount = 0;

        var avatarLogicEntity = internalEntity.OutFacing;

        avatarLogicEntity.OnPositionChanged += () =>
        {
            log.Debug("Position Changed! {RollMode} {Position}", internalEntity.RollMode, internalEntity.Self.position);
            Assert.Equal(expectedRollMode, internalEntity.RollMode);
            if (expectedRollMode == EntityRollMode.Rollback)
            {
                rollbackPositionChangedCount++;
            }

            positionChangedCount++;
        };

        avatarLogicEntity.OnAmmoCountChanged += () =>
        {
            Assert.Equal(expectedRollMode, internalEntity.RollMode);
            log.Debug("Ammo Changed! {RollMode}", internalEntity.RollMode);
            ammoChangedCount++;
        };

        var spawnedEntity = new Entity(new(99), internalEntity);
        var rollbackStack = new RollbackStack();
        var predictedInputs = new LogicalInputQueue();
        var mockInputFetch = new MockInputFetch();

        var now = new TickId(23);

        var positionBefore = internalEntity.Self.position;
        var positionAt26 = new Position3();
        var ammoAt26 = 0;
        const int expectedPredictCount = 10;
        for (var i = 0; i < 10; ++i)
        {
            if (i == 4)
            {
                mockInputFetch.PrimaryAbility = true;
            }

            if (i == 3)
            {
                positionAt26 = internalEntity.Self.position;
                ammoAt26 = internalEntity.Self.ammoCount;
            }

            var localPlayerIndex = new LocalPlayerIndex(1);

            var inputPack = mockInputFetch.Fetch(localPlayerIndex);

            internalEntity.SetInput(new OctetReader(inputPack));

            var logicalInputPack = new LogicalInput(localPlayerIndex, now, inputPack);
            predictedInputs.AddLogicalInput(logicalInputPack);

            PredictionTicker.Predict(spawnedEntity, now, rollbackStack, PredictMode.Predicting);
            now = now.Next();
            log.Debug("Prediction at {Now} {Position} {Ammo}", now, internalEntity.Self.position,
                internalEntity.Self.ammoCount);
        }

        Assert.Equal(expectedPredictCount, rollbackStack.Count);
        Assert.Equal(expectedPredictCount, positionChangedCount);
        var positionAfter = internalEntity.Self.position;
        Assert.NotEqual(positionAfter, positionBefore);

        expectedRollMode = EntityRollMode.Rollback;

        const int expectedRollbackCount = 7;
        var rollbackTargetTickId = new TickId(26);
        log.Debug("rolling back to {TickId}", rollbackTargetTickId);
        RollBacker.Rollback(spawnedEntity, rollbackStack, rollbackTargetTickId);
        log.Debug("Rolled back to at {TargetTickId} {Position} {Ammo}", rollbackTargetTickId,
            internalEntity.Self.position, internalEntity.Self.ammoCount);
        Assert.Equal(positionAt26, internalEntity.Self.position);
        Assert.Equal(ammoAt26, internalEntity.Self.ammoCount);
        Assert.Equal(expectedPredictCount + expectedRollbackCount, positionChangedCount);
        Assert.Equal(expectedRollbackCount, rollbackPositionChangedCount);
        Assert.Equal(2, ammoChangedCount);
        predictedInputs.DiscardUpToAndExcluding(rollbackTargetTickId);

        expectedRollMode = EntityRollMode.Rollforth;
        const int expectedRollforthCount = 7;
        var predictionStateHistory = new PredictionStateChecksumQueue();
        Assert.Equal(20, internalEntity.Self.ammoCount);
        RollForth.Rollforth(spawnedEntity, predictedInputs, rollbackStack, predictionStateHistory);
        Assert.Equal(positionAfter, internalEntity.Self.position);
        Assert.Equal(19, internalEntity.Self.ammoCount);
        Assert.Equal(expectedRollforthCount, predictionStateHistory.Count);
        Assert.Equal(27u, predictionStateHistory.FirstTickId.tickId);
        Assert.Equal(expectedPredictCount + expectedRollbackCount + expectedRollforthCount, positionChangedCount);
    }
}