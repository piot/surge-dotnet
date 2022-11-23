/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.LocalPlayer;

namespace Piot.Surge.LogicalInput
{
    public interface IInputFetch<out T>
    {
        T FetchInput(LocalPlayerIndex playerIndex);
    }
}