/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.FieldMask;

namespace Piot.Surge.DeltaSnapshot
{
    /// <summary>
    ///     Information about an entity that changed any of its field between two snapshots.
    ///     Does only contain the <c>changeMask</c>, but should probably be refactored to hold the serialized data as well.
    /// </summary>
    public struct ChangedEntity
    {
        public ChangedFieldsMask changeMask;
        public EntityId entityId;

        public ChangedEntity(EntityId entityId, ChangedFieldsMask changeMask)
        {
            this.entityId = entityId;
            this.changeMask = changeMask;
        }
    }
}