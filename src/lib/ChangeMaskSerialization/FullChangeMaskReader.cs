/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;
using Piot.Surge.OctetSerialize;

namespace Piot.Surge.ChangeMaskSerialization
{
    public static class FullChangeMaskReader
    {
        public static FullChangeMask ReadFullChangeMask(IOctetReader reader)
        {
            var v = reader.ReadUInt16();
            ulong completeMask = v;
            if ((v & 0x8000) == 0) return new FullChangeMask { mask = completeMask };

            var next = reader.ReadUInt16();
            completeMask <<= 16;
            completeMask |= next;

            return new FullChangeMask { mask = completeMask };
        }
    }
}