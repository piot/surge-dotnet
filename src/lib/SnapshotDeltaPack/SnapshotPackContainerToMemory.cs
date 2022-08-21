/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    ///     Holds the deleted, created and updated memory packs.
    /// </summary>
    public struct SnapshotDeltaMemory
    {
        public TickId tickId;
        public uint deletedCount;
        public ReadOnlyMemory<byte> deletedMemory;

        public uint createdCount;
        public ReadOnlyMemory<byte> createdMemory;

        public uint updatedCount;
        public ReadOnlyMemory<byte> updatedMemory;

        public uint correctionCount;
        public ReadOnlyMemory<byte> correctionMemory;
    }

    public static class SnapshotPackContainerToMemory
    {
        private static (uint, ReadOnlyMemory<byte>) PackAllExcept(IReadPackContainer containerToRead,
            uint[] excludeEntityIds)
        {
            uint count = 0;
            var target = new OctetWriter(Constants.MaxDatagramOctetSize);

            foreach (var pair in containerToRead.Entries.Where(pair => !excludeEntityIds.Contains(pair.Key)))
            {
                count++;
                target.WriteOctets(pair.Value.Span);
            }

            return (count, target.Octets);
        }

        private static (uint, ReadOnlyMemory<byte>) PackOnly(IReadPackContainer containerToRead, uint[] includeEntities)
        {
            uint count = 0;
            var target = new OctetWriter(Constants.MaxDatagramOctetSize);

            foreach (var pair in containerToRead.Entries.Where(pair => includeEntities.Contains(pair.Key)))
            {
                count++;
                target.WriteOctets(pair.Value.Span);
            }

            return (count, target.Octets);
        }

        /// <summary>
        ///     Packs together from a container (that has reusable entity packs), the deleted, updated and created entities.
        ///     It also filters out the entities specified in <paramref name="excludeEntityIds" />.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="excludeEntityIds">
        ///     The entities that should not be included in the pack. Usually because
        ///     the correction entity states will be appended later.
        /// </param>
        /// <returns></returns>
        public static SnapshotDeltaMemory PackWithFilter(DeltaSnapshotPackContainer container,
            EntityId[] excludeEntityIds)
        {
            var flattenedUInts = excludeEntityIds.Select(entityId => entityId.Value).ToArray();

            var (createdMemoryCount, createMemory) =
                PackAllExcept(container.CreatedEntityContainerRead, flattenedUInts);
            var (updatedMemoryCount, updatedMemory) =
                PackAllExcept(container.EntityUpdateContainerRead, flattenedUInts);
            var (deletedMemoryCount, deletedMemory) =
                PackAllExcept(container.DeletedEntityContainerRead, flattenedUInts);
            var (correctionCount, correctionMemory) = PackOnly(container.CorrectionEntityContainerRead, flattenedUInts);

            return new SnapshotDeltaMemory
            {
                tickId = container.TickId,
                createdCount = createdMemoryCount,
                createdMemory = createMemory,
                updatedCount = updatedMemoryCount,
                updatedMemory = updatedMemory,
                deletedCount = deletedMemoryCount,
                deletedMemory = deletedMemory,
                correctionCount = correctionCount,
                correctionMemory = correctionMemory
            };
        }
    }
}