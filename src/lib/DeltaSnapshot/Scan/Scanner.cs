/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.Scan
{
    public static class Scanner
    {
        /// <summary>
        ///     Scans an entity container for all changes and creates a snapshot delta internal.
        /// </summary>
        /// <param name="world">entity container with change information</param>
        /// <param name="tickId"></param>
        /// <returns></returns>
        public static DeltaSnapshotEntityIds Scan(IEntityContainerWithDetectChanges world, TickId tickId)
        {
            var deletedEntities = world.Deleted.Select(entity => entity.Id).ToArray();

            var createdEntities = world.Created
                .Select(entity => entity.Id).ToArray();

            var updatedEntities = (from entity in world.AllEntities
                where entity.Mode == EntityMode.Normal
                let changes = entity.CompleteEntity.Changes()
                select new ChangedEntity(entity.Id, new(changes))).ToArray();

            return new(tickId, deletedEntities, createdEntities, updatedEntities);
        }
    }
}