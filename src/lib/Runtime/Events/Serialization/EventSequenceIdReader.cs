/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event.Serialization
{
    public static class EventSequenceIdReader
    {
        public static EventSequenceId Read(IBitReader reader)
        {
            return new((ushort)reader.ReadBits(16));
        }

        public static EventSequenceId Read(IOctetReader reader)
        {
            return new(reader.ReadUInt16());
        }
    }
}