/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.ChangeMask;
using Piot.Surge.SnapshotDelta;

namespace Piot.Surge.SnapshotDeltaInternal
{
    /// <summary>
    ///     Keeps track of all the changes that has happened to the World. It only stores the Entity Ids
    ///     and FullChangeMasks for updated entities,
    ///     not the actual field values, archetypeIds, etc.
    /// </summary>
    public class SnapshotDeltaInternal
    {
        public readonly Dictionary<ulong, SnapshotDeltaInternalInfoEntity> entities = new();

        public SnapshotDeltaInternal()
        {
        }

        public SnapshotDeltaInternal(EntityId[] deletedIds, EntityId[] createdIds,
            SnapshotDeltaChangedEntity[] updatedEntities)
        {
            foreach (var deletedId in deletedIds)
            {
                if (entities.ContainsKey(deletedId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[deletedId.Value] =
                    new SnapshotDeltaInternalInfoEntity(deletedId, ChangedFieldsMask.DeletedMaskBit);
            }

            foreach (var createdId in createdIds)
            {
                if (entities.ContainsKey(createdId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[createdId.Value] =
                    new SnapshotDeltaInternalInfoEntity(createdId, ChangedFieldsMask.AllFieldChangedMaskBits);
            }

            foreach (var updatedEntity in updatedEntities)
            {
                if (entities.ContainsKey(updatedEntity.entityId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[updatedEntity.entityId.Value] =
                    new SnapshotDeltaInternalInfoEntity(updatedEntity.entityId, updatedEntity.changeMask.mask);
            }
        }

        public SnapshotDeltaInternalInfoEntity FetchEntity(EntityId entityId)
        {
            return entities[entityId.Value];
        }
    }
}