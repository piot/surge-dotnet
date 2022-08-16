/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Flood;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.SnapshotDeltaPack
{
    internal static class SnapshotDeltaToPackContainer
    {
        /// <summary>
        ///     Creates a pack from a snapshot delta and then drops the changes from the entity container (world).
        /// </summary>
        /// <param name="world"></param>
        /// <param name="snapshotDeltaAfter"></param>
        /// <returns></returns>
        internal static DeltaSnapshotPackContainer SnapshotDeltaToContainer(IEntityContainer world, SnapshotDelta.SnapshotDelta snapshotDeltaAfter)
        {
            var targetPackContainer = new DeltaSnapshotPackContainer();
            
            var createdEntities = snapshotDeltaAfter.createdIds.Select(world.FetchEntity).ToArray();
            foreach (var createdEntity in createdEntities)
            {
                var writer = new OctetWriter(256);
                PackCreatedEntity.Write(writer, createdEntity.Id, createdEntity.ArchetypeId, createdEntity);
                targetPackContainer.CreatedEntityContainer.Add(createdEntity.Id, writer.Octets);
            }

            var updatedEntities = snapshotDeltaAfter.updatedEntities.Select(x =>
                (IUpdatedEntity)new UpdatedEntity(x.entityId, x.changeMask, world.FetchEntity(x.entityId))).ToArray();
            foreach (var updateEntity in updatedEntities)
            {
                var writer = new OctetWriter(256);
                PackUpdatedEntity.Write(writer, updateEntity.Id, updateEntity.ChangeMask, updateEntity.Serializer);
                targetPackContainer.EntityUpdateContainer.Add(updateEntity.Id, writer.Octets);
            }

            foreach (var deletedEntityId in snapshotDeltaAfter.deletedIds)
            {
                var writer = new OctetWriter(256);
                EntityIdWriter.Write(writer, deletedEntityId);
                targetPackContainer.DeletedEntityContainer.Add(deletedEntityId, writer.Octets);
            }
            
            targetPackContainer.TickId = snapshotDeltaAfter.TickId;

            return targetPackContainer;
        }
    }
}