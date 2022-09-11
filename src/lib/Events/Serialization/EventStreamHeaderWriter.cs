/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;

namespace Piot.Surge.Events.Serialization
{
    public static class EventStreamHeaderWriter
    {
        public static void Write(IBitWriter writer, EventStreamPackItem[] events)
        {
            #if DEBUG
            BitMarker.WriteMarker(writer, Constants.ShortLivedEventsStartSync);
            #endif
            writer.WriteBits((byte)events.Length, 6);

            if (events.Length > 0)
            {
                EventSequenceIdWriter.Write(writer, events[0].eventSequenceId);
            }
        }
    }
}