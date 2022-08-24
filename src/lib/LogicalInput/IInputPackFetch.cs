/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.LogicalInput
{
    public interface IInputPackFetch
    {
        public ReadOnlySpan<byte> Fetch(LocalPlayerIndex playerIndex);
    }
}