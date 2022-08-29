/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.ChangeMask;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDelta;
using Piot.Surge.SnapshotDeltaMasks;

namespace Piot.Surge.SnapshotDeltaInternal
{

    public static class SnapshotDeltaToEntityMasks
    {
        /// <summary>
        ///     Converts from the created, updated and deletedEntityIds to set the masks of all the changes that has happened to the World. It only stores the <see cref="EntityId" />s
        ///     and <see cref="ChangedFieldsMask" /> for updated entities,
        ///     not the actual field values, archetypeIds, etc.
        /// </summary>
        public static SnapshotDeltaEntityMasks ConvertToEntityMasks(SnapshotDelta.SnapshotDelta snapshot)
        {
            var entities = new Dictionary<ushort, ulong>();
            
            foreach (var deletedId in snapshot.deletedIds)
            {
                if (entities.ContainsKey(deletedId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[deletedId.Value] = ChangedFieldsMask.DeletedMaskBit;
            }

            foreach (var createdId in snapshot.createdIds)
            {
                if (entities.ContainsKey(createdId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[createdId.Value] = ChangedFieldsMask.AllFieldChangedMaskBits;
            }

            foreach (var updatedEntity in snapshot.updatedEntities)
            {
                if (entities.ContainsKey(updatedEntity.entityId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[updatedEntity.entityId.Value] = updatedEntity.changeMask.mask;
            }

            return new(snapshot.TickId, entities);
        }
    }
}