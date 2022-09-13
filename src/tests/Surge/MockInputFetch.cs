/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Tests.ExampleGame;

namespace Tests;

public sealed class MockInputFetch : IInputPackFetch
{
    private readonly ILog log;
    private bool primaryAbility;

    private bool secondaryAbility;

    public MockInputFetch(ILog log)
    {
        this.log = log;
    }

    public bool PrimaryAbility
    {
        set => primaryAbility = value;
    }

    public bool SecondaryAbility
    {
        set => secondaryAbility = value;
    }

    public ReadOnlySpan<byte> Fetch(LocalPlayerIndex playerIndex)
    {
        var input = new GameInput
        {
            aiming = default,
            primaryAbility = primaryAbility,
            secondaryAbility = secondaryAbility,
            tertiaryAbility = false,
            ultimateAbility = false,
            desiredMovement = default
        };

        var writer = new OctetWriter(30);

        GameInputWriter.Write(writer, input);

        return writer.Octets;
    }
}