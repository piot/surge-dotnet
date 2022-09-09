/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Piot.Flood;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;
using Piot.Surge.SnapshotDeltaPack.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.Types.Serialization;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Piot.Surge.DeltaSnapshot.Pack.Convert
{
    public static class DeltaSnapshotToBitPack
    {
        /// <summary>
        ///     Creates a pack from a deltaSnapshotEntityIds delta.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="deltaSnapshotEntityIds"></param>
        /// <returns></returns>
        public static DeltaSnapshotPack ToDeltaSnapshotPack(IEntityContainer world,
            IEventWithArchetypeAndSequenceId[] shortLivedEvents,
            DeltaSnapshotEntityIds deltaSnapshotEntityIds, TickIdRange tickIdRange)
        {
            var writer = new BitWriter(Constants.MaxSnapshotOctetSize);

#if DEBUG
            writer.WriteBits(Constants.SnapshotDeltaSync, 8);
#endif
            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.deletedIds.Length, writer);
            foreach (var deletedEntityId in deltaSnapshotEntityIds.deletedIds)
            {
                EntityIdWriter.Write(writer, deletedEntityId);
            }
#if DEBUG
            writer.WriteBits(Constants.SnapshotDeltaCreatedSync, 8);
#endif
            var createdEntities = deltaSnapshotEntityIds.createdIds.Select(world.FetchEntity).ToArray();
            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.createdIds.Length, writer);

            foreach (var createdEntity in createdEntities)
            {
                PackCreatedEntity.Write(writer, createdEntity.Id, createdEntity.ArchetypeId,
                    createdEntity.CompleteEntity);
            }
#if DEBUG
            writer.WriteBits(Constants.SnapshotDeltaUpdatedSync, 8);
#endif
            var updatedEntities = deltaSnapshotEntityIds.updatedEntities.Select(x =>
                (IUpdatedEntity)new UpdatedEntity(x.entityId, x.changeMask,
                    world.FetchEntity(x.entityId).CompleteEntity)).ToArray();
            EntityCountWriter.WriteEntityCount((uint)deltaSnapshotEntityIds.updatedEntities.Length, writer);
            foreach (var updateEntity in updatedEntities)
            {
                PackUpdatedEntity.Write(writer, updateEntity.Id, updateEntity.ChangeMask, updateEntity.Serializer);
            }

            EventsWriter.Write(shortLivedEvents, writer);

            return new(tickIdRange, writer.Close(out _),
                SnapshotStreamType.BitStream, SnapshotType.DeltaSnapshot);
        }
    }
}