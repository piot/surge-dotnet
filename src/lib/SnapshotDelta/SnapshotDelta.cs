/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.SnapshotDelta
{
    public class SnapshotDelta
    {
        public EntityId[] createdIds;
        public EntityId[] deletedIds;
        public SnapshotDeltaChangedEntity[] updatedEntities;

        public SnapshotDelta(EntityId[] deletedIds, EntityId[] createdIds,
            SnapshotDeltaChangedEntity[] updatedEntities)
        {
            this.deletedIds = deletedIds;
            this.createdIds = createdIds;
            this.updatedEntities = updatedEntities;
        }
    }
}