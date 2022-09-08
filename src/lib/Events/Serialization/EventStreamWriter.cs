/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.Event.Serialization
{
    public static class EventStreamWriter
    {
        public static void Write(EventStream stream, TickIdRange tickIdRange, IOctetWriter writer)
        {
            var events = stream.FetchEventsForRange(tickIdRange);
            EventsWriter.Write(events, writer);
        }
    }
}