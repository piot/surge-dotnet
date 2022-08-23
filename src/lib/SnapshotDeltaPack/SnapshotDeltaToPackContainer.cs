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
            SnapshotDelta.SnapshotDelta snapshotDeltaAfter, EntityId[] correctionEntityIds)
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
                targetPackContainer.EntityUpdateContainer.Add(updateEntity.Id, writer.Octets.ToArray());
            }

            foreach (var deletedEntityId in snapshotDeltaAfter.deletedIds)
            {
                var writer = new OctetWriter(256);
                EntityIdWriter.Write(writer, deletedEntityId);
                targetPackContainer.DeletedEntityContainer.Add(deletedEntityId, writer.Octets.ToArray());
            }

            var correctedEntities = correctionEntityIds.Select(correctedEntityId =>
                    (ICorrectedEntity)new CorrectedEntity(correctedEntityId, new LocalPlayerIndex(0),
                        world.FetchEntity(correctedEntityId)))
                .ToArray();

            foreach (var correctedEntity in correctedEntities)
            {
                var writer = new OctetWriter(256);
                EntityIdWriter.Write(writer, correctedEntity.Id);
                LocalPlayerIndexWriter.Write(correctedEntity.ControlledByLocalPlayerIndex, writer);
                correctedEntity.SerializeAll(writer);
                correctedEntity.SerializeCorrectionState(writer);
                targetPackContainer.CorrectionEntityContainer.Add(correctedEntity.Id, writer.Octets.ToArray());
            }

            targetPackContainer.TickId = snapshotDeltaAfter.TickId;

            return targetPackContainer;
        }
    }
}