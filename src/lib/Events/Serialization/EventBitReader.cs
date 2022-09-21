/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event.Serialization
{
    public static class EventBitReader
    {
        public static EventSequenceId ReadAndApply(IBitReader reader, IEventProcessor eventProcessor,
            EventSequenceId nextExpectedSequenceId)
        {
            var (count, startSequenceId) = EventStreamHeaderReader.Read(reader);
            var sequenceId = startSequenceId;

            for (var i = 0; i < count; ++i)
            {
                if (!sequenceId.IsEqualOrSuccessor(nextExpectedSequenceId))
                {
                    sequenceId = sequenceId.Next;
                    eventProcessor.SkipOneEvent(reader);
                    continue;
                }

                eventProcessor.ReadAndApply(reader);
                sequenceId = sequenceId.Next;
                nextExpectedSequenceId = sequenceId;
            }

            return nextExpectedSequenceId;
        }

        public static EventSequenceId Skip(IBitReader reader, IEventProcessor eventProcessor,
            EventSequenceId nextExpectedSequenceId)
        {
            var (count, startSequenceId) = EventStreamHeaderReader.Read(reader);
            var sequenceId = startSequenceId;

            for (var i = 0; i < count; ++i)
            {
                if (!sequenceId.IsEqualOrSuccessor(nextExpectedSequenceId))
                {
                    sequenceId = sequenceId.Next;
                    eventProcessor.SkipOneEvent(reader);
                    continue;
                }

                eventProcessor.SkipOneEvent(reader);
                sequenceId = sequenceId.Next;
                nextExpectedSequenceId = sequenceId;
            }

            return nextExpectedSequenceId;
        }
    }
}