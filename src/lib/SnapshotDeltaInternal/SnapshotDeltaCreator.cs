/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Surge.ChangeMask;
using Piot.Surge.SnapshotDelta;

namespace Piot.Surge.SnapshotDeltaInternal
{
    public static class SnapshotDeltaCreator
    {
        /// <summary>
        ///     Scans an entity container for all changes and creates a snapshot delta internal.
        /// </summary>
        /// <param name="world">entity container with change information</param>
        /// <returns></returns>
        public static SnapshotDeltaInternal Scan(IEntityContainerWithChanges world)
        {
            var deletedEntities = world.Deleted.Select(entity => entity.Id).ToArray();

            var createdEntities = world.Created
                .Select(entity => entity.Id).ToArray();

            var updatedEntities = (from entity in world.AllEntities
                where entity.Mode == EntityMode.Normal
                let changes = entity.GeneratedEntity.Changes()
                select new SnapshotDeltaChangedEntity(entity.Id, new FullChangeMask(changes))).ToArray();

            var snapshotDelta = new SnapshotDeltaInternal(deletedEntities, createdEntities, updatedEntities);

            return snapshotDelta;
        }
    }
}