/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.Types;

namespace Tests.Surge.ExampleGame;

public enum ExplodeType
{
    Fireball,
    Lightning
}

[ShortLivedEvents]
public interface IShortEvents
{
    public void Explode(Position3 position, byte magnitude); //  ExplodeType explodeType,
}