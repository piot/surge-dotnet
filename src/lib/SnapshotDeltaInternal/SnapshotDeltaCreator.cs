/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Surge.ChangeMask;
using Piot.Surge.SnapshotDelta;

namespace Piot.Surge.SnapshotDeltaInternal
{
    public class SnapshotDeltaCreator
    {
        public static SnapshotDeltaInternal Scan(World world)
        {
            var deletedEntities = world.Deleted.Select(entity => entity.Id).ToArray();

            var createdEntities = world.Created
                .Select(entity => entity.Id).ToArray();

            var updatedEntities = (from entity in world.Entities.Values.ToArray()
                where entity.Mode == EntityMode.Normal
                let changes = entity.GeneratedEntity.Changes()
                select new SnapshotDeltaChangedEntity(entity.Id, new FullChangeMask(changes))).ToArray();

            var snapshotDelta = new SnapshotDeltaInternal(deletedEntities, createdEntities, updatedEntities);

            return snapshotDelta;
        }
    }
}