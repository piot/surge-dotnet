/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.SnapshotProtocol;

namespace Piot.Surge.Event.Serialization
{
    public static class EventsWriter
    {
        public static void Write(IEventWithArchetypeAndSequenceId[] events, IOctetWriter writer)
        {
            OctetMarker.WriteMarker(writer, Constants.ShortLivedEventsStartSync);

            writer.WriteUInt8((byte)events.Length);

            if (events.Length == 0)
            {
                return;
            }

            writer.WriteUInt16(events[0].SequenceId.sequenceId);

            foreach (var shortLivedEvent in events)
            {
                EventWriter.Write(shortLivedEvent, writer);
            }
        }

        public static void Write(IEventWithArchetypeAndSequenceId[] events, IBitWriter writer)
        {
            BitMarker.WriteMarker(writer, Constants.ShortLivedEventsStartSync);
            writer.WriteBits((byte)events.Length, 6);

            if (events.Length > 0)
            {
                writer.WriteBits(events[0].SequenceId.sequenceId, 16);
            }

            foreach (var shortLivedEvent in events)
            {
                EventWriter.Write(shortLivedEvent, writer);
            }
        }
    }
}