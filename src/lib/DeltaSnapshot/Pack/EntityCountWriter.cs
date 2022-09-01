/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    public static class EntityCountWriter
    {
        public static void WriteEntityCount(uint count, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)count);
        }

        public static void WriteEntityCount(uint count, IBitWriter writer)
        {
            writer.WriteBits((ushort)count, 16);
        }
    }
}