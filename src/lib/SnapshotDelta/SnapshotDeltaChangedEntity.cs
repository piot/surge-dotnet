/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;

namespace Piot.Surge.SnapshotDelta
{
    /**
     * Information about an entity that changed any of its field between two snapshots.
     * Does only contain the changeMask, but should probably be refactored to hold the serialized data as well.
     */
    public struct SnapshotDeltaChangedEntity
    {
        public ChangedFieldsMask changeMask;
        public EntityId entityId;

        public SnapshotDeltaChangedEntity(EntityId entityId, ChangedFieldsMask changeMask)
        {
            this.entityId = entityId;
            this.changeMask = changeMask;
        }
    }
}