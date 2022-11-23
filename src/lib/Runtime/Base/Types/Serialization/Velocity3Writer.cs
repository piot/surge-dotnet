/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity3Writer
    {
        public static void Write(IBitWriter writer, Velocity3 velocity)
        {
            BitWriterUtils.WriteSignedBits(writer, velocity.x, 12);
            BitWriterUtils.WriteSignedBits(writer, velocity.y, 12);
            BitWriterUtils.WriteSignedBits(writer, velocity.x, 12);
        }
    }
}