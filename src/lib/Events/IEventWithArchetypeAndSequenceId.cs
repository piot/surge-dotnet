/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Event
{
    public interface IEventWithArchetypeAndSequenceId : IEventWithArchetype
    {
        public EventSequenceId SequenceId { get; }
    }
}