/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class UnitVector2Reader
    {
        public static UnitVector2 Read(IOctetReader reader)
        {
            return new()
            {
                x = reader.ReadInt32(),
                y = reader.ReadInt32()
            };
        }

        public static UnitVector2 Read(IBitReader reader)
        {
            return new()
            {
                x = BitReaderUtils.ReadSignedBits(reader, 20),
                y = BitReaderUtils.ReadSignedBits(reader, 20)
            };
        }
    }
}