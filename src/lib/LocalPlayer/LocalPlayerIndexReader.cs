/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.LocalPlayer
{
    public static class LocalPlayerIndexReader
    {
        public static LocalPlayerIndex Read(IOctetReader reader)
        {
            return new LocalPlayerIndex(reader.ReadUInt8());
        }
    }
}