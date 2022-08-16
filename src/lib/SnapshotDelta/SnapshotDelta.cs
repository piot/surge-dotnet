/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDelta
{
    /// <summary>
    ///     Holds information about what has changed between two snapshots
    ///     Used as input for the serialization.
    /// </summary>
    public class SnapshotDelta
    {
        public readonly EntityId[] createdIds;
        public readonly EntityId[] deletedIds;
        public readonly SnapshotDeltaChangedEntity[] updatedEntities;

        public SnapshotDelta(TickId tickId, EntityId[] deletedIds, EntityId[] createdIds,
            SnapshotDeltaChangedEntity[] updatedEntities)
        {
            TickId = tickId;
            this.deletedIds = deletedIds;
            this.createdIds = createdIds;
            this.updatedEntities = updatedEntities;
        }

        public TickId TickId { get; }
    }
}