/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.LocalPlayer.Serialization
{
    public static class LocalPlayerIndexWriter
    {
        public static void Write(LocalPlayerIndex playerIndex, IBitWriter writer)
        {
            writer.WriteBits(playerIndex.Value, 4);
        }
    }
}