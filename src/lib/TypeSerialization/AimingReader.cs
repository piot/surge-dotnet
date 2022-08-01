/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Types;

namespace Piot.Surge
{
    public static class AimingReader
    {
        public static Aiming Read(IOctetReader reader)
        {
            var yaw = reader.ReadUInt16();
            var pitch = reader.ReadInt16();

            return new Aiming(yaw, pitch);
        }
    }
}