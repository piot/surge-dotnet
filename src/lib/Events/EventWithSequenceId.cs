/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event
{
    public class EventWithSequenceId : IEventWithArchetypeAndSequenceId
    {
        public EventWithSequenceId(EventSequenceId eventSequenceId,
            IEventWithArchetype eventWithArchetypeEventWithArchetype)
        {
            SequenceId = eventSequenceId;
            EventWithArchetype = eventWithArchetypeEventWithArchetype;
        }

        public IEventWithArchetype EventWithArchetype { get; }
        public EventSequenceId SequenceId { get; }

        public void Serialize(IBitWriter bitWriter)
        {
            EventWithArchetype.Serialize(bitWriter);
        }

        public void DeSerialize(IBitReader bitReader)
        {
            EventWithArchetype.DeSerialize(bitReader);
        }

        public void Serialize(IOctetWriter octetWriter)
        {
            EventWithArchetype.Serialize(octetWriter);
        }

        public void DeSerialize(IOctetReader octetReader)
        {
            EventWithArchetype.DeSerialize(octetReader);
        }

        public EventArchetypeId ArchetypeId => EventWithArchetype.ArchetypeId;
    }
}