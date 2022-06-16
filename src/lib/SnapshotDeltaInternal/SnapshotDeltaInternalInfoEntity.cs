/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.SnapshotDeltaInternal
{
    public class SnapshotDeltaInternalInfoEntity
    {
        public ulong changeMask;
        public EntityId entityId;

        public SnapshotDeltaInternalInfoEntity(EntityId entityId, ulong changeMask)
        {
            this.entityId = entityId;
            this.changeMask = changeMask;
        }
    }
}