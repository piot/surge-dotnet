/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Event
{
    public interface IEventWithArchetype : IEventSerializer
    {
        public EventArchetypeId ArchetypeId { get; }
    }
}