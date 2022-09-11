/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaBitReader
    {
        /// <summary>
        ///     Reading a snapshot delta pack and returning the created, deleted and updated entities.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entityContainer"></param>
        /// <returns></returns>
        public static EventSequenceId ReadAndApply(IBitReader reader,
            IEntityContainerWithGhostCreator entityGhostContainerWithCreator,
            IEventProcessor eventProcessor, EventSequenceId nextExpectedSequenceId,
            bool isOverlappingMergedSnapshot)
        {
            SnapshotDeltaEntityBitReader.ReadAndApply(reader, entityGhostContainerWithCreator,
                isOverlappingMergedSnapshot);

            var (count, startSequenceId) = EventStreamHeaderReader.Read(reader);
            var sequenceId = startSequenceId;

            for (var i = 0; i < count; ++i)
            {
                if (!sequenceId.IsEqualOrSuccessor(nextExpectedSequenceId))
                {
                    sequenceId = sequenceId.Next();
                    eventProcessor.SkipOneEvent(reader);
                    continue;
                }

                eventProcessor.ReadAndApply(reader);
                sequenceId = sequenceId.Next();
                nextExpectedSequenceId = sequenceId;
            }

            return nextExpectedSequenceId;
        }
    }
}