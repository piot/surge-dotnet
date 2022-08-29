/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Flood;
using Piot.Surge.LocalPlayer;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotDeltaPack
{
    internal static class SnapshotDeltaToPackContainer
    {
        /// <summary>
        ///     Creates a pack from a snapshot delta.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="snapshotDeltaAfter"></param>
        /// <returns></returns>
        internal static DeltaSnapshotPackContainer SnapshotDeltaToContainer(IEntityContainer world,
            SnapshotDelta.SnapshotDelta snapshotDeltaAfter)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);

            foreach (var deletedEntityId in snapshotDeltaAfter.deletedIds)
            {
                EntityIdWriter.Write(writer, deletedEntityId);
            }
            
            var createdEntities = snapshotDeltaAfter.createdIds.Select(world.FetchEntity).ToArray();
            foreach (var createdEntity in createdEntities)
            {
                PackCreatedEntity.Write(writer, createdEntity.Id, createdEntity.ArchetypeId, createdEntity);
            }

            var updatedEntities = snapshotDeltaAfter.updatedEntities.Select(x =>
                (IUpdatedEntity)new UpdatedEntity(x.entityId, x.changeMask, world.FetchEntity(x.entityId))).ToArray();
            foreach (var updateEntity in updatedEntities)
            {
                PackUpdatedEntity.Write(writer, updateEntity.Id, updateEntity.ChangeMask, updateEntity.Serializer);
            }

            return new DeltaSnapshotPackContainer(snapshotDeltaAfter.TickId, writer.Octets);
        }
    }
}