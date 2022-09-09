/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event.Serialization
{
    public static class EventSequenceIdWriter
    {
        public static void Write(IBitWriter bitWriter, EventSequenceId sequenceId)
        {
            bitWriter.WriteBits(sequenceId.sequenceId, 16);
        }

        public static void Write(IOctetWriter octetWriter, EventSequenceId sequenceId)
        {
            octetWriter.WriteUInt16(sequenceId.sequenceId);
        }
    }
}