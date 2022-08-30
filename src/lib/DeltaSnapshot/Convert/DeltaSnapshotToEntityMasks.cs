/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.FieldMask;
using Piot.Surge.DeltaSnapshot.EntityMask;

namespace Piot.Surge.DeltaSnapshot.Convert
{
    public static class DeltaSnapshotToEntityMasks
    {
        /// <summary>
        ///     Converts from the created, updated and deletedEntityIds to set the masks of all the changes that has happened to
        ///     the World. It only stores the <see cref="EntityId" />s
        ///     and <see cref="ChangedFieldsMask" /> for updated entities,
        ///     not the actual field values, archetypeIds, etc.
        /// </summary>
        public static EntityMasks ToEntityMasks(DeltaSnapshotEntityIds deltaSnapshotEntityIds)
        {
            var entities = new Dictionary<ushort, ulong>();

            foreach (var deletedId in deltaSnapshotEntityIds.deletedIds)
            {
                if (entities.ContainsKey(deletedId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[deletedId.Value] = ChangedFieldsMask.DeletedMaskBit;
            }

            foreach (var createdId in deltaSnapshotEntityIds.createdIds)
            {
                if (entities.ContainsKey(createdId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[createdId.Value] = ChangedFieldsMask.AllFieldChangedMaskBits;
            }

            foreach (var updatedEntity in deltaSnapshotEntityIds.updatedEntities)
            {
                if (entities.ContainsKey(updatedEntity.entityId.Value))
                {
                    throw new Exception("EntityId can only be included once");
                }

                entities[updatedEntity.entityId.Value] = updatedEntity.changeMask.mask;
            }

            return new(deltaSnapshotEntityIds.TickId, entities);
        }
    }
}