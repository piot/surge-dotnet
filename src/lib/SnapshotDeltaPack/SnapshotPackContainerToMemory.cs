using System;
using System.Linq;
using Piot.Flood;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    public struct SnapshotDeltaMemory
    {
        public TickId tickId;
        public uint deletedCount;
        public Memory<byte> deletedMemory;

        public uint createdCount;
        public Memory<byte> createdMemory;

        public uint updatedCount;
        public Memory<byte> updatedMemory;
    }
    
    public static class SnapshotPackContainerToMemory
    {
        private static (uint, Memory<byte>) Pack(IReadPackContainer containerToRead, ulong[] excludeEntityIds)
        {
            uint count = 0;
            var target = new OctetWriter(Constants.MaxDatagramOctetSize);
            
            foreach (var pair in containerToRead.Entries.Where(pair => !excludeEntityIds.Contains(pair.Key)))
            {
                count++;
                target.WriteOctets(pair.Value);
            }

            return (count, target.Octets);
        }
        
        public static SnapshotDeltaMemory PackWithFilter(DeltaSnapshotPackContainer container, EntityId[] excludeEntityIds)
        {
            var flattenedUInts = excludeEntityIds.Select(entityId => entityId.Value).ToArray();
         
            var (createdMemoryCount, createMemory) = Pack(container.CreatedEntityContainerRead, flattenedUInts);
            var (updatedMemoryCount, updatedMemory) = Pack(container.EntityUpdateContainerRead, flattenedUInts);
            var (deletedMemoryCount, deletedMemory) = Pack(container.DeletedEntityContainerRead, flattenedUInts);

            return new SnapshotDeltaMemory
            {
                tickId = container.TickId,
                createdCount = createdMemoryCount,
                createdMemory = createMemory,
                updatedCount = updatedMemoryCount,
                updatedMemory = updatedMemory,
                deletedCount = deletedMemoryCount,
                deletedMemory = deletedMemory,
            };
        }
    }
}