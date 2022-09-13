/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Types;

namespace Tests.Surge.ExampleGame;

public sealed class ShortEvents : IShortEvents
{
    private readonly ILog log;

    public ShortEvents(ILog log)
    {
        this.log = log;
    }

    public void Explode(Position3 position, byte magnitude)
    {
        log.Info("Explode {Position}, {Magnitude}", position, magnitude);
    }
}