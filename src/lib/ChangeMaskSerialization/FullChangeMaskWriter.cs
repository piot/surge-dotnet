/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;
using Piot.Flood;

namespace Piot.Surge.ChangeMaskSerialization
{
    public static class FullChangeMaskWriter
    {
        public static void WriteFullChangeMask(IOctetWriter writer, FullChangeMask changeMask)
        {
            writer.WriteUInt16((ushort)((changeMask.mask & 0x7fff) | 0x8000));
            writer.WriteUInt16((ushort)((changeMask.mask >> 16) & 0xffff));
        }
    }
}