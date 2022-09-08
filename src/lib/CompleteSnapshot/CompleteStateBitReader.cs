/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateBitReader
    {
        public static void ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator,
            IEventProcessorWithCreate eventProcessorWithCreate)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.CompleteSnapshotStartMarker);
#endif
            CompleteStateEntityBitReader.Apply(reader, entityGhostContainerWithCreator);
            var eventsForThisRange = EventStreamReader.Read(eventProcessorWithCreate, reader);

            eventProcessorWithCreate.PerformEvents(eventsForThisRange);
        }
    }
}