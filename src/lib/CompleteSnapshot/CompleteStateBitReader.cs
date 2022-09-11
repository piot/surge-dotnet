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
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator,
            IEventProcessor eventProcessor)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.CompleteSnapshotStartMarker);
#endif
            CompleteStateEntityBitReader.Apply(reader, entityGhostContainerWithCreator);

            var nextExpectedShortLivedEventSequenceId = EventSequenceIdReader.Read(reader);

            var (eventCount, firstSequenceId) = EventStreamHeaderReader.Read(reader);

            var sequenceId = firstSequenceId;
            for (var i = 0; i < eventCount; ++i)
            {
                eventProcessor.ReadAndApply(reader);

                sequenceId = sequenceId.Next();
                nextExpectedShortLivedEventSequenceId = sequenceId;
            }

            return nextExpectedShortLivedEventSequenceId;
        }
    }
}