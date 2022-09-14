/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity3Writer
    {
        public static void Write(Velocity3 velocity, IOctetWriter writer)
        {
            writer.WriteInt16((short)velocity.x);
            writer.WriteInt16((short)velocity.y);
            writer.WriteInt16((short)velocity.z);
        }

        public static void Write(Velocity3 velocity, IBitWriter writer)
        {
            BitWriterUtils.WriteSignedBits(writer, velocity.x, 12);
            BitWriterUtils.WriteSignedBits(writer, velocity.y, 12);
            BitWriterUtils.WriteSignedBits(writer, velocity.x, 12);
        }
    }
}