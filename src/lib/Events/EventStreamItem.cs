/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Tick;

namespace Piot.Surge.Event
{
    public struct EventStreamItem
    {
        public TickId tickId;
        public IEventWithArchetypeAndSequenceId @event;
    }
}