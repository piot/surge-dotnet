/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class UnitVector2Writer
    {
        public static void Write(IBitWriter writer, UnitVector2 position)
        {
            BitWriterUtils.WriteSignedBits(writer, position.x, 20);
            BitWriterUtils.WriteSignedBits(writer, position.y, 20);
        }
    }
}