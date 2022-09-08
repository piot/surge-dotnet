/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.SnapshotProtocol;

namespace Piot.Surge.Event.Serialization
{
    public static class EventStreamReader
    {
        public static IEventWithArchetypeAndSequenceId[] Read(IEventCreator creator, IOctetReader reader)
        {
            var count = reader.ReadUInt8();
            var createdEvents = new List<IEventWithArchetypeAndSequenceId>();

            if (count == 0)
            {
                return Array.Empty<IEventWithArchetypeAndSequenceId>();
            }

            var sequenceId = reader.ReadUInt16();

            for (var i = 0; i < count; ++i)
            {
                var archetypeId = reader.ReadUInt8();
                var createdEvent = creator.Create(new(archetypeId));
                var wrappedEvent = new EventWithSequenceId(new(sequenceId), createdEvent);

                createdEvent.DeSerialize(reader);
                createdEvents.Add(wrappedEvent);
                sequenceId++;
            }

            return createdEvents.ToArray();
        }

        public static IEventWithArchetypeAndSequenceId[] Read(IEventCreator creator, IBitReader reader)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.ShortLivedEventsStartSync);
#endif
            var count = reader.ReadBits(6);
            if (count == 0)
            {
                return Array.Empty<IEventWithArchetypeAndSequenceId>();
            }

            var createdEvents = new List<IEventWithArchetypeAndSequenceId>();

            var sequenceId = reader.ReadBits(16);
            for (var i = 0; i < count; ++i)
            {
                var archetypeId = reader.ReadBits(6);
                var createdEvent = creator.Create(new((byte)archetypeId));
                var wrappedEvent = new EventWithSequenceId(new((ushort)sequenceId), createdEvent);
                createdEvent.DeSerialize(reader);
                createdEvents.Add(wrappedEvent);
                sequenceId++;
            }

            return createdEvents.ToArray();
        }
    }
}