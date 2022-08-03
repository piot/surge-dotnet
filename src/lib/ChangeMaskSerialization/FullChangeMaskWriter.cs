/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;
using Piot.Flood;

namespace Piot.Surge.ChangeMaskSerialization
{
    public static class Constants
    {
        public const byte FullChangeMaskSync = 0x91;
    }
    public static class FullChangeMaskWriter
    {
        public static void WriteFullChangeMask(IOctetWriter writer, FullChangeMask changeMask)
        {
            #if DEBUG
            writer.WriteUInt8(Constants.FullChangeMaskSync);
            #endif
            writer.WriteUInt16((ushort)((changeMask.mask >> 16) & 0xffff | 0x8000));
            writer.WriteUInt16((ushort)(changeMask.mask & 0x7fff));
        }
    }
}