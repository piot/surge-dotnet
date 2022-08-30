/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.FieldMask;

namespace Piot.Surge.FieldMask.Serialization
{
    public static class Constants
    {
        public const byte FullChangeMaskSync = 0x91;
    }

    public static class ChangedFieldsMaskWriter
    {
        public static void WriteChangedFieldsMask(IOctetWriter writer, ChangedFieldsMask changeMask)
        {
#if DEBUG
            writer.WriteUInt8(Constants.FullChangeMaskSync);
#endif
            writer.WriteUInt16((ushort)(((changeMask.mask >> 15) & 0x7fff) | 0x8000));
            writer.WriteUInt16((ushort)(changeMask.mask & 0xffff));
        }
    }
}