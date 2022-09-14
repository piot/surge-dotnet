/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Types;

namespace Tests.ExampleGame;

[Input]
public struct GameInput
{
    public Aiming aiming;
    public bool primaryAbility;
    public bool secondaryAbility;
    public bool tertiaryAbility;

    public bool ultimateAbility;
    public Velocity2 desiredMovement;
}

public class GameInputFetch
{
    public static GameInput ReadFromDevice(LocalPlayerIndex _)
    {
        return new()
        {
            primaryAbility = true,
            secondaryAbility = true
        };
    }
}