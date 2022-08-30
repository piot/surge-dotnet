/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Flood;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.Tick;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.DeltaSnapshot.Pack.Convert
{
    public static class DeltaSnapshotToPack
    {
        /// <summary>
        ///     Creates a pack from a deltaSnapshotEntityIds delta.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="deltaSnapshotEntityIds"></param>
        /// <returns></returns>
        public static DeltaSnapshotPack ToDeltaSnapshotPack(IEntityContainer world,
            DeltaSnapshotEntityIds deltaSnapshotEntityIds)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);

            writer.WriteUInt8(Constants.SnapshotDeltaSync);

            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.deletedIds.Length, writer);
            foreach (var deletedEntityId in deltaSnapshotEntityIds.deletedIds)
            {
                EntityIdWriter.Write(writer, deletedEntityId);
            }

            writer.WriteUInt8(Constants.SnapshotDeltaCreatedSync);

            var createdEntities = deltaSnapshotEntityIds.createdIds.Select(world.FetchEntity).ToArray();
            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.createdIds.Length, writer);

            foreach (var createdEntity in createdEntities)
            {
                PackCreatedEntity.Write(writer, createdEntity.Id, createdEntity.ArchetypeId, createdEntity);
            }

            writer.WriteUInt8(Constants.SnapshotDeltaUpdatedSync);
            var updatedEntities = deltaSnapshotEntityIds.updatedEntities.Select(x =>
                (IUpdatedEntity)new UpdatedEntity(x.entityId, x.changeMask, world.FetchEntity(x.entityId))).ToArray();
            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.updatedEntities.Length, writer);
            foreach (var updateEntity in updatedEntities)
            {
                PackUpdatedEntity.Write(writer, updateEntity.Id, updateEntity.ChangeMask, updateEntity.Serializer);
            }

            return new(TickIdRange.FromTickId(deltaSnapshotEntityIds.TickId), writer.Octets);
        }
    }
}