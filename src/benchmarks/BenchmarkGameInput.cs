/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Types;

namespace Benchmark.Surge.ExampleGame;

[Input]
public struct BenchmarkGameInput
{
    public Aiming aiming;
    public bool primaryAbility;
    public bool secondaryAbility;
    public bool tertiaryAbility;

    public bool ultimateAbility;
    public Velocity2 desiredMovement;
}

public static class GameInputFetch
{
    public static BenchmarkGameInput ReadFromDevice(LocalPlayerIndex _)
    {
        return new()
        {
            primaryAbility = true,
            secondaryAbility = true
        };
    }
}