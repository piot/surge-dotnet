/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.DatagramType
{
    public static class DatagramTypeWriter
    {
        public static void Write(IOctetWriter writer, DatagramType type)
        {
            writer.WriteUInt8((byte)type);
        }
    }
}