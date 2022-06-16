/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.Types;

namespace Tests.ExampleGame;

[Input]
public struct GameInput
{
    public Aiming aiming;
    public bool fire;
    public bool jump;
    public ushort throttle;
}