/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class AimingWriter
    {
        public static void Write(IBitWriter writer, Aiming aiming)
        {
            writer.WriteBits(aiming.yaw, 16);
            BitWriterUtils.WriteSignedBits(writer, aiming.pitch, 16);
        }
    }
}