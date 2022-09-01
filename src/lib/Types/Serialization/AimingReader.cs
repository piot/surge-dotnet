/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class AimingReader
    {
        public static Aiming Read(IOctetReader reader)
        {
            var yaw = reader.ReadUInt16();
            var pitch = reader.ReadInt16();

            return new Aiming(yaw, pitch);
        }

        public static Aiming Read(IBitReader reader)
        {
            var yaw = (ushort)reader.ReadBits(16);
            var pitch = (short)reader.ReadBits(16);

            return new Aiming(yaw, pitch);
        }
    }
}