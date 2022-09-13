/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Types;

namespace Benchmark.Surge.ExampleGame;

public sealed class BenchmarkShortEvents : IBenchmarkShortEvents
{
    private readonly ILog log;

    public BenchmarkShortEvents(ILog log)
    {
        this.log = log;
    }

    public void Explode(Position3 position, byte magnitude)
    {
        log.Info("Explode {Position}, {Magnitude}", position, magnitude);
    }
}