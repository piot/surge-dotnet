/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.LocalPlayer.Serialization
{
    public static class LocalPlayerIndexReader
    {
        public static LocalPlayerIndex Read(IOctetReader reader)
        {
            return new(reader.ReadUInt8());
        }

        public static LocalPlayerIndex Read(IBitReader reader)
        {
            return new((byte)reader.ReadBits(4));
        }
    }
}