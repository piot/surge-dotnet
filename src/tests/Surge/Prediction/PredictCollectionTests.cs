/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Tick;
using Xunit.Abstractions;

namespace Tests.ExampleGame;

public sealed class PredictionCollectionTests
{
    readonly ILog log;

    public PredictionCollectionTests(ITestOutputHelper output)
    {
        var logTarget = new TestOutputLogger(output);

        var combinedLogTarget = new CombinedLogTarget(new ILogTarget[] { logTarget, new ConsoleOutputLogger() });
        log = new Log(combinedLogTarget, LogLevel.LowLevel);
    }

    [Fact]
    public void PredictCollectionEnqueueAndPop()
    {
        var collection = new PredictCollection();

        var undoPack = ReadOnlySpan<byte>.Empty;
        var inputPack = new byte[] { 0x1f };
        var logicStatePack = new byte[] { 0x2a, 0x2b, 0x2c };
        var physicsStatePack = ReadOnlySpan<byte>.Empty;
        var tickId = new TickId(40);

        Assert.Empty(collection);
        collection.EnqueuePredict(tickId, undoPack, inputPack, logicStatePack, physicsStatePack);
        Assert.Single(collection);

        Assert.Equal(tickId, collection.FirstTickId);
        Assert.Equal(tickId, collection.LastTickId);
        Assert.Equal(tickId, collection.TickId);

        var rollback = collection.PopRollback();
        Assert.Empty(collection);
        Assert.Throws<Exception>(() => collection.TickId);
        Assert.Throws<InvalidOperationException>(() => collection.LastTickId);
        Assert.Equal(tickId, rollback.tickId);
        Assert.Equal(logicStatePack.ToArray(), rollback.logicStatePack.ToArray());
    }

    [Fact]
    public void PredictCollectionEnqueue2AndPop()
    {
        var collection = new PredictCollection();

        var undoPack = ReadOnlySpan<byte>.Empty;
        var inputPack = new byte[] { 0x1f };
        var logicStatePack = new byte[] { 0x2a, 0x2b, 0x2c };
        var physicsStatePack = ReadOnlySpan<byte>.Empty;
        var tickId = new TickId(40);

        Assert.Empty(collection);
        collection.EnqueuePredict(tickId, undoPack, inputPack, logicStatePack, physicsStatePack);
        Assert.Single(collection);

        var secondTickId = tickId.Next;

        var logicState2Pack = new byte[] { 0x3a, 0x3b, 0x3c };
        collection.EnqueuePredict(secondTickId, undoPack, inputPack, logicState2Pack, physicsStatePack);
        Assert.Equal(2, collection.Count);

        Assert.Equal(tickId, collection.FirstTickId);
        Assert.Equal(secondTickId, collection.LastTickId);
        Assert.Equal(secondTickId, collection.TickId);

        var rollback = collection.PopRollback();
        Assert.Single(collection);
        Assert.Equal(secondTickId, rollback.tickId);
        Assert.Equal(logicState2Pack.ToArray(), rollback.logicStatePack.ToArray());
    }

    [Fact]
    public void PredictCollectionMultiEnqueueAndPop()
    {
        var collection = new PredictCollection();

        var undoPack = ReadOnlySpan<byte>.Empty;
        var inputPack = new byte[] { 0x1f };

        var physicsStatePack = ReadOnlySpan<byte>.Empty;
        var firstTickId = new TickId(40);

        Assert.Empty(collection);

        var tickId = firstTickId;

        const int count = 32;

        for (var i = 0; i < count; ++i)
        {
            var logicStatePack = new byte[] { 0x2a, (byte)i, 0x2c };
            collection.EnqueuePredict(tickId, undoPack, inputPack, logicStatePack, physicsStatePack);
            Assert.Equal(i + 1, collection.Count);
            tickId = tickId.Next;
        }

        Assert.Equal(firstTickId, collection.FirstTickId);
        Assert.Equal(tickId.Previous, collection.LastTickId);
        Assert.Equal(tickId.Previous, collection.TickId);

        var rollback = collection.PopRollback();
        Assert.Equal(count - 1, collection.Count);
        Assert.Equal(tickId.Previous, rollback.tickId);
        var expectedLogicStatePack = new byte[] { 0x2a, count - 1, 0x2c };
        Assert.Equal(expectedLogicStatePack.ToArray(), rollback.logicStatePack.ToArray());
    }

    [Fact]
    public void PredictCollectionTraverseEnqueueAndPop()
    {
        var collection = new PredictCollection();

        var undoPack = ReadOnlySpan<byte>.Empty;
        var inputPack = new byte[] { 0x1f };

        var physicsStatePack = ReadOnlySpan<byte>.Empty;
        var firstTickId = new TickId(40);

        Assert.Empty(collection);

        var tickId = firstTickId;

        const int Count = 32;

        for (var i = 0; i < Count; ++i)
        {
            var logicStatePack = new byte[] { 0x2a, (byte)i, 0x2c };
            collection.EnqueuePredict(tickId, undoPack, inputPack, logicStatePack, physicsStatePack);
            Assert.Equal(i + 1, collection.Count);
            tickId = tickId.Next;
        }

        Assert.Equal(firstTickId, collection.FirstTickId);
        Assert.Equal(tickId.Previous, collection.LastTickId);
        Assert.Equal(tickId.Previous, collection.TickId);

        var predict = collection.Current;
        Assert.Equal(tickId.Previous, predict.tickId);

        collection.MovePrevious();
        var rollback = collection.Current;
        Assert.Equal(Count, collection.Count);
        Assert.Equal(tickId.Previous.Previous, rollback.tickId);
        var expectedLogicStatePack = new byte[] { 0x2a, Count - 2, 0x2c };
        Assert.Equal(expectedLogicStatePack.ToArray(), rollback.logicStatePack.ToArray());
    }
}