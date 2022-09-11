/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Event;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateBitReader
    {
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator,
            IEventProcessor eventProcessor)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.CompleteSnapshotStartMarker);
#endif
            CompleteStateEntityBitReader.Apply(reader, entityGhostContainerWithCreator);

            return EventCompleteBitReader.ReadAndApply(eventProcessor, reader);
        }
    }
}