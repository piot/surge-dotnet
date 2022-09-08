/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event.Serialization
{
    public static class EventWriter
    {
        public static void Write(IEventWithArchetype eventWithArchetype, IOctetWriter writer)
        {
            writer.WriteUInt8(eventWithArchetype.ArchetypeId.archetype);
            eventWithArchetype.Serialize(writer);
        }

        public static void Write(IEventWithArchetype eventWithArchetype, IBitWriter writer)
        {
            writer.WriteBits(eventWithArchetype.ArchetypeId.archetype, 6);
            eventWithArchetype.Serialize(writer);
        }
    }
}