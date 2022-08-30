/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class AimingWriter
    {
        public static void Write(Aiming aiming, IOctetWriter writer)
        {
            writer.WriteUInt16(aiming.yaw);
            writer.WriteInt16(aiming.pitch);
        }
    }
}