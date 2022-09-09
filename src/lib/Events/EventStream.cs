/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Surge.Tick;

namespace Piot.Surge.Event
{
    /// <summary>
    ///     Holds events in a queue to be replicated as a stream.
    /// </summary>
    public class EventStream
    {
        private readonly Queue<EventStreamItem> events = new();

        private bool isInitialized;
        private TickId lastInsertedTickId;
        private ushort sequenceId;

        public IEnumerable<EventStreamItem> Events => events;
        public EventSequenceId NextSequenceId => new(sequenceId);

        /// <summary>
        ///     Adds an event at the specified <paramref name="tickId" />. The events must be enqueued with the same
        ///     or higher tickId than the last one enqueued.
        /// </summary>
        /// <param name="tickId"></param>
        /// <param name="eventWithArchetype"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Enqueue(TickId tickId, IEventWithArchetype eventWithArchetype)
        {
            if (isInitialized)
            {
                if (tickId < lastInsertedTickId)
                {
                    throw new ArgumentOutOfRangeException(nameof(tickId),
                        "must have same or increasing tickIds in event stream");
                }
            }

            events.Enqueue(new()
            {
                tickId = tickId,
                @event = new EventWithSequenceId(new(sequenceId), eventWithArchetype)
            });

            lastInsertedTickId = tickId;
            sequenceId++;
            isInitialized = true;
        }

        /// <summary>
        ///     Removes events that are before or at <paramref name="tickId" />.
        /// </summary>
        /// <param name="tickId"></param>
        public void DiscardIncluding(TickId tickId)
        {
            while (events.Any())
            {
                if (events.Peek().tickId > tickId)
                {
                    break;
                }

                events.Dequeue();
            }
        }

        /// <summary>
        ///     Fetches all events that are stored within the given <paramref name="tickIdRange" />.
        /// </summary>
        /// <param name="tickIdRange"></param>
        /// <returns></returns>
        public IEventWithArchetypeAndSequenceId[] FetchEventsForRange(TickIdRange tickIdRange)
        {
            var matchingEvents = new List<IEventWithArchetypeAndSequenceId>();
            foreach (var shortLivedEvent in events)
            {
                var peekTickId = shortLivedEvent.tickId;

                if (peekTickId < tickIdRange.startTickId)
                {
                    continue;
                }

                if (peekTickId > tickIdRange.Last)
                {
                    break;
                }

                matchingEvents.Add(shortLivedEvent.@event);
            }

            return matchingEvents.ToArray();
        }
    }
}