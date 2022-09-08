/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Event.Serialization;

namespace Piot.Surge.Event
{
    public interface IEventProcessorWithCreate : IEventCreator
    {
        public void PerformEvents(IEventWithArchetype[] shortLivedEvents);
        public void PerformEvent(IEventWithArchetype shortLivedEvent);
    }
}