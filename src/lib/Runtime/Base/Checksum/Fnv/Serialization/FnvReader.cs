/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Checksum.Fnv.Serialization
{
    public static class FnvReader
    {
        public static uint Read(IOctetReader reader)
        {
            return reader.ReadUInt32();
        }
    }
}