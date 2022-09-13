/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.Event
{
    /// <summary>
    ///     Holds events in a queue to be replicated as a stream.
    /// </summary>
    public sealed class EventStreamPackQueue
    {
        private readonly Queue<EventStreamPackItem> events = new();
        private TickId authoritativeTickId;

        private BitWriter bitWriterForCurrentTick = new(1024);

        private bool isInitialized;
        private TickId lastInsertedTickId;
        private ushort sequenceId;

        public EventStreamPackQueue(TickId authoritativeTickId)
        {
            this.authoritativeTickId = authoritativeTickId;
        }

        public IEnumerable<EventStreamPackItem> Events => events;
        public EventSequenceId NextSequenceId => new(sequenceId);

        public IBitWriter BitWriter => bitWriterForCurrentTick;

        /// <summary>
        ///     Adds an event at the specified <paramref name="tickId" />. The events must be enqueued with the same
        ///     or higher tickId than the last one enqueued.
        /// </summary>
        /// <param name="tickId"></param>
        /// <param name="eventWithArchetype"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Enqueue(TickId tickId, ReadOnlySpan<byte> payload, uint bitCount)
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
                eventSequenceId = new(sequenceId),
                payload = payload.ToArray(),
                bitCount = bitCount
            });

            lastInsertedTickId = tickId;
            sequenceId++;
            isInitialized = true;
        }

        public void EndOfTick(TickId expectedAuthoritativeTickId)
        {
            authoritativeTickId = expectedAuthoritativeTickId;
            if (bitWriterForCurrentTick.BitPosition <= 0)
            {
                return;
            }

            var octets = bitWriterForCurrentTick.Close(out var bitPosition);
            Enqueue(authoritativeTickId, octets, (uint)bitPosition);
            bitWriterForCurrentTick = new BitWriter(1024);
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
        public EventStreamPackItem[] FetchEventsForRange(TickIdRange tickIdRange)
        {
            var matchingEvents = new List<EventStreamPackItem>(events.Count);

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

                matchingEvents.Add(shortLivedEvent);
            }

            return matchingEvents.ToArray();
        }
    }
}