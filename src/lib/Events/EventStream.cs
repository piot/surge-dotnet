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
    public class EventStream
    {
        private readonly Queue<EventStreamItem> events = new();

        private bool isInitialized;
        private TickId lastInsertedTickId;
        public ushort sequenceId;

        public IEnumerable<EventStreamItem> Events => events;

        public void Add(TickId tickId, IEventWithArchetype eventWithArchetype)
        {
            if (isInitialized)
            {
                if (tickId != lastInsertedTickId && !tickId.IsImmediateFollowing(lastInsertedTickId))
                {
                    throw new ArgumentOutOfRangeException(nameof(tickId),
                        "must have same or consecutive tickIds in event stream");
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