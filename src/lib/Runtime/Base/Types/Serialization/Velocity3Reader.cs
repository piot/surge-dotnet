/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity3Reader
    {
        public static void Read(IBitReader reader, ref Velocity3 data)
        {
            data.x = BitReaderUtils.ReadSignedBits(reader, 12);
            data.y = BitReaderUtils.ReadSignedBits(reader, 12);
            data.z = BitReaderUtils.ReadSignedBits(reader, 12);
        }
    }
}