/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;

namespace Piot.Surge.SnapshotDelta
{
    public struct SnapshotDeltaChangedEntity
    {
        public FullChangeMask changeMask;
        public EntityId entityId;

        public SnapshotDeltaChangedEntity(EntityId entityId, FullChangeMask changeMask)
        {
            this.entityId = entityId;
            this.changeMask = changeMask;
        }
    }
}