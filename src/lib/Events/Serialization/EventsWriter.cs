/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Events.Serialization;

namespace Piot.Surge.Event.Serialization
{
    public static class EventsWriter
    {
        public static void Write(EventStreamPackItem[] events, IBitWriter writer)
        {
            EventStreamHeaderWriter.Write(writer, events);
            
            foreach (var shortLivedEvent in events)
            {
                var reader = new BitReader(shortLivedEvent.payload.Span, (int)shortLivedEvent.bitCount);
                writer.CopyBits(reader, (int)shortLivedEvent.bitCount);
            }
        }
    }
}