/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.DatagramType.Serialization
{
    public static class DatagramTypeReader
    {
        public static DatagramType Read(IOctetReader reader)
        {
            return (DatagramType)reader.ReadUInt8();
        }
    }
}