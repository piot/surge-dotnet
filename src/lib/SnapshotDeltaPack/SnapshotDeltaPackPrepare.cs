/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Surge.SnapshotDelta;

namespace Piot.Surge.SnapshotDeltaPack
{
    public static class SnapshotDeltaPackPrepare
    {
        public static (IEntity[], IUpdatedEntity[]) Prepare(EntityId[] deltaCreatedEntities,
            SnapshotDeltaChangedEntity[] deltaUpdatedEntities, IEntityContainer world)
        {
            var createdEntities = deltaCreatedEntities.Select(world.FetchEntity).ToArray();

            var updatedEntities = deltaUpdatedEntities
                .Select(updated => new UpdateEntity(updated.changeMask, world.FetchEntity(updated.entityId))).ToArray();

            return (createdEntities, updatedEntities);
        }
    }
}