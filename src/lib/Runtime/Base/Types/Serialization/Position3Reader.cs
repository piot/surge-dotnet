/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Position3Reader
    {
        public static void Read(IBitReader reader, ref Position3 data)
        {
            data.x = BitReaderUtils.ReadSignedBits(reader, 20);
            data.y = BitReaderUtils.ReadSignedBits(reader, 20);
            data.z = BitReaderUtils.ReadSignedBits(reader, 20);
        }
    }
}