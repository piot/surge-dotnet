using System;
using System.Collections.Generic;

namespace Piot.Surge.SnapshotDeltaPack
{
    public interface IFeedEntityPackToContainer
    {
        public void Add(EntityId entityId, Memory<byte> payload);
    }

    public interface IReadPackContainer
    {
        public Dictionary<ulong, Memory<byte>> Entries { get; }
    }
    public class SnapshotEntityPackContainer : IFeedEntityPackToContainer, IReadPackContainer
    {
        readonly Dictionary<ulong, Memory<byte>> entityPacks = new ();
        
        public Dictionary<ulong, Memory<byte>> Entries => entityPacks;
        
        public void Add(EntityId entityId, Memory<byte> payload)
        {
            if (entityPacks.ContainsKey(entityId.Value))
            {
                throw new Exception($"pack for {entityId} already inserted");
            }
            entityPacks.Add(entityId.Value, payload);
        }
        
        public Memory<byte> PackForEntity(EntityId entityId)
        {
            var wasFound = entityPacks.TryGetValue(entityId.Value, out var foundPack);
            if (!wasFound)
            {
                throw new Exception($"no pack for entity {entityId}");
            }

            return foundPack;
        }
    }
}