/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class AimingReader
    {
        public static void Read(IBitReader reader, ref Aiming data)
        {
            data.yaw = (ushort)reader.ReadBits(16);
            data.pitch = (short)BitReaderUtils.ReadSignedBits(reader, 16);
        }
    }
}