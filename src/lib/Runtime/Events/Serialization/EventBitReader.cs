/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Core;

namespace Piot.Surge.Event.Serialization
{
    public static class EventBitReader
    {
        public static EventSequenceId ReadAndApply(IBitReader reader, IEventReceiver eventProcessor,
            EventSequenceId nextExpectedSequenceId)
        {
            var (count, startSequenceId) = EventStreamHeaderReader.Read(reader);
            var sequenceId = startSequenceId;

            for (var i = 0; i < count; ++i)
            {
                var eventTypeId = reader.ReadBits(8);
                if (!sequenceId.IsEqualOrSuccessor(nextExpectedSequenceId))
                {
                    sequenceId = sequenceId.Next;
                    EventStreamReceiver.Skip(reader, eventTypeId);
                    continue;
                }


                EventStreamReceiver.ReceiveFull(reader, eventTypeId, eventProcessor);
                sequenceId = sequenceId.Next;
                nextExpectedSequenceId = sequenceId;
            }

            return nextExpectedSequenceId;
        }

        public static EventSequenceId Skip(IBitReader reader, IEventReceiver eventProcessor,
            EventSequenceId nextExpectedSequenceId)
        {
            var (count, startSequenceId) = EventStreamHeaderReader.Read(reader);
            var sequenceId = startSequenceId;

            for (var i = 0; i < count; ++i)
            {
                var eventTypeId = reader.ReadBits(8);
                if (!sequenceId.IsEqualOrSuccessor(nextExpectedSequenceId))
                {
                    sequenceId = sequenceId.Next;
                    EventStreamReceiver.Skip(reader, eventTypeId);
                    continue;
                }

                EventStreamReceiver.ReceiveFull(reader, eventTypeId, eventProcessor);
                sequenceId = sequenceId.Next;
                nextExpectedSequenceId = sequenceId;
            }

            return nextExpectedSequenceId;
        }
    }
}