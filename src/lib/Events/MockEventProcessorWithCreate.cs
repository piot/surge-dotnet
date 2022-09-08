/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;

namespace Piot.Surge.Event
{
    public class MockEventWithArchetype : IEventWithArchetype
    {
        public MockEventWithArchetype(EventArchetypeId archetypeId)
        {
            ArchetypeId = archetypeId;
        }

        public EventArchetypeId ArchetypeId { get; }

        public void Serialize(IBitWriter bitWriter)
        {
            bitWriter.WriteBits(0x1d, 5);
        }

        public void DeSerialize(IBitReader bitReader)
        {
            if (bitReader.ReadBits(5) != 0x1d)
            {
                throw new Exception("wrong bits read");
            }
        }

        public void Serialize(IOctetWriter octetWriter)
        {
            octetWriter.WriteUInt8(0x1d);
        }

        public void DeSerialize(IOctetReader octetReader)
        {
            if (octetReader.ReadUInt8() != 0x1d)
            {
                throw new Exception("wrong bits read");
            }
        }
    }

    public class MockEventProcessorWithCreate : IEventProcessorWithCreate
    {
        private readonly ILog log;
//        private ushort sequenceId;

        public MockEventProcessorWithCreate(ILog log)
        {
            this.log = log;
        }

        public void PerformEvents(IEventWithArchetype[] shortLivedEvents)
        {
            foreach (var shortLivedEvent in shortLivedEvents)
            {
                log.DebugLowLevel("performing event of {ArchetypeId}", shortLivedEvent.ArchetypeId);
            }
        }

        public IEventWithArchetype Create(EventArchetypeId archetypeId)
        {
            log.DebugLowLevel("creating mock event of {ArchetypeId}", archetypeId);
            return new MockEventWithArchetype(archetypeId);
        }
    }
}