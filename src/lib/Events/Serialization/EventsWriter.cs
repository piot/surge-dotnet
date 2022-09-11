/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event.Serialization
{
    public static class EventsWriter
    {
        public static void Write(EventStreamPackItem[] events, IBitWriter writer)
        {
            BitMarker.WriteMarker(writer, Constants.ShortLivedEventsStartSync);
            writer.WriteBits((byte)events.Length, 6);

            if (events.Length > 0)
            {
                EventSequenceIdWriter.Write(writer, events[0].eventSequenceId);
            }

            foreach (var shortLivedEvent in events)
            {
                var reader = new BitReader(shortLivedEvent.payload.Span, (int)shortLivedEvent.bitCount);
                writer.CopyBits(reader, (int)shortLivedEvent.bitCount);
            }
        }
    }
}