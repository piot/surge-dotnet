/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.ChangeMask;
using Piot.Surge.SnapshotDelta;

namespace Piot.Surge.SnapshotDeltaInternal
{
    /// <summary>
    ///     Helper class for putting together all the changes between two ticks/snapshots
    /// </summary>
    public static class FromSnapshotDeltaInternal
    {
        public static SnapshotDelta.SnapshotDelta Convert(SnapshotDeltaInternal internalDelta)
        {
            var deletedIds = new List<EntityId>();
            var createdIds = new List<EntityId>();
            var updatedEntities = new List<SnapshotDeltaChangedEntity>();

            foreach (var x in internalDelta.entities)
            {
                var changeMask = x.Value.changeMask;
                if (changeMask == ChangedFieldsMask.AllFieldChangedMaskBits)
                {
                    createdIds.Add(new EntityId(x.Key));
                }
                else if ((changeMask & ChangedFieldsMask.DeletedMaskBit) != 0)
                {
                    deletedIds.Add(new EntityId(x.Key));
                }
                else
                {
                    updatedEntities.Add(new SnapshotDeltaChangedEntity(new EntityId(x.Key),
                        new ChangedFieldsMask(x.Value.changeMask)));
                }
            }

            return new SnapshotDelta.SnapshotDelta(internalDelta.TickId, deletedIds.ToArray(), createdIds.ToArray(),
                updatedEntities.ToArray());
        }
    }
}