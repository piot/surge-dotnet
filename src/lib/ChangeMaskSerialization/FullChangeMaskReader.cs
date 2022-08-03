/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.ChangeMask;
using Piot.Flood;

namespace Piot.Surge.ChangeMaskSerialization
{
    public static class FullChangeMaskReader
    {
        public static FullChangeMask ReadFullChangeMask(IOctetReader reader)
        {
            #if DEBUG
            if (reader.ReadUInt8() != Constants.FullChangeMaskSync)
            {
                throw new Exception("out of sync");
            }
            #endif
            var v = reader.ReadUInt16();
            if ((v & 0x8000) == 0) return new FullChangeMask { mask = v };
            ulong completeMask = (ushort) (v & 0x7fff);
            

            var next = reader.ReadUInt16();
            completeMask <<= 16;
            completeMask |= next;

            return new FullChangeMask { mask = completeMask };
        }
    }
}