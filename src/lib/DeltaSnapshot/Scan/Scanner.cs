/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Surge.FieldMask;
using Piot.Surge.Tick;
using Piot.Surge.DeltaSnapshot;
using Piot.Surge.Entities;

namespace Piot.Surge.DeltaSnapshot.Scan
{
    public static class Scanner
    {
        /// <summary>
        ///     Scans an entity container for all changes and creates a snapshot delta internal.
        /// </summary>
        /// <param name="world">entity container with change information</param>
        /// <returns></returns>
        public static DeltaSnapshotEntityIds Scan(IEntityContainerWithDetectChanges world, TickId tickId)
        {
            var deletedEntities = world.Deleted.Select(entity => entity.Id).ToArray();

            var createdEntities = world.Created
                .Select(entity => entity.Id).ToArray();

            var updatedEntities = (from entity in world.AllEntities
                where entity.Mode == EntityMode.Normal
                let changes = entity.GeneratedEntity.Changes()
                select new ChangedEntity(entity.Id, new (changes))).ToArray();

            return new DeltaSnapshotEntityIds(tickId, deletedEntities, createdEntities, updatedEntities);
        }
    }
}