/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class EventCompleteBitReader
    {
        public static EventSequenceId ReadAndApply(IEventReceiver eventProcessor, IBitReader reader)
        {
            var nextExpectedShortLivedEventSequenceId = EventSequenceIdReader.Read(reader);

            var (eventCount, firstSequenceId) = EventStreamHeaderReader.Read(reader);

            var sequenceId = firstSequenceId;
            for (var i = 0; i < eventCount; ++i)
            {
                var eventTypeId = reader.ReadBits(8);
                EventStreamReceiver.ReceiveFull(reader, eventTypeId, eventProcessor);

                sequenceId = sequenceId.Next;
                nextExpectedShortLivedEventSequenceId = sequenceId;
            }

            return nextExpectedShortLivedEventSequenceId;
        }

        public static EventSequenceId Skip(IBitReader reader)
        {
            var nextExpectedShortLivedEventSequenceId = EventSequenceIdReader.Read(reader);

            var (eventCount, firstSequenceId) = EventStreamHeaderReader.Read(reader);

            var sequenceId = firstSequenceId;
            for (var i = 0; i < eventCount; ++i)
            {
                var eventTypeId = reader.ReadBits(8);
                EventStreamReceiver.Skip(reader, eventTypeId);

                sequenceId = sequenceId.Next;
                nextExpectedShortLivedEventSequenceId = sequenceId;
            }

            return nextExpectedShortLivedEventSequenceId;
        }
    }
}