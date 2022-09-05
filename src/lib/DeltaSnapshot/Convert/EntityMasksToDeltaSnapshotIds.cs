/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.FieldMask;

namespace Piot.Surge.DeltaSnapshot.Convert
{
    public static class EntityMasksToDeltaSnapshotIds
    {
        public static DeltaSnapshotEntityIds ToEntityIds(EntityMasksUnion masks)
        {
            var deletedIds = new List<EntityId>();
            var createdIds = new List<EntityId>();
            var changedEntities = new List<ChangedEntity>();

            foreach (var maskPair in masks.EntityMasks)
            {
                var mask = maskPair.Value;

                if (mask == ChangedFieldsMask.DeletedMaskBit)
                {
                    deletedIds.Add(new(maskPair.Key));
                }
                else if (mask == ChangedFieldsMask.AllFieldChangedMaskBits)
                {
                    createdIds.Add(new(maskPair.Key));
                }
                else
                {
                    changedEntities.Add(new(new(maskPair.Key), new(maskPair.Value)));
                }
            }

            return new(masks.TickIdRange.Last, deletedIds.ToArray(), createdIds.ToArray(), changedEntities.ToArray());
        }
    }
}