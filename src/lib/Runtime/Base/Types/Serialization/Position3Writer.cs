/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    [BitSerializer]
    public static class Position3Writer
    {
        public static void Write(IBitWriter writer, Position3 position)
        {
            BitWriterUtils.WriteSignedBits(writer, position.x, 20);
            BitWriterUtils.WriteSignedBits(writer, position.y, 20);
            BitWriterUtils.WriteSignedBits(writer, position.z, 20);
        }
    }
}