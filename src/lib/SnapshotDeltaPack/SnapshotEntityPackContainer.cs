/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

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
        public Dictionary<uint, Memory<byte>> Entries { get; }
    }

    /// <summary>
    ///     Holds reusable packs of either updated, deleted or created serialized entities.
    /// </summary>
    public class SnapshotEntityPackContainer : IFeedEntityPackToContainer, IReadPackContainer
    {
        public void Add(EntityId entityId, Memory<byte> payload)
        {
            if (Entries.ContainsKey(entityId.Value))
            {
                throw new Exception($"pack for {entityId} already inserted");
            }

            Entries.Add(entityId.Value, payload);
        }

        public Dictionary<uint, Memory<byte>> Entries { get; } = new();

        public Memory<byte> PackForEntity(EntityId entityId)
        {
            var wasFound = Entries.TryGetValue(entityId.Value, out var foundPack);
            if (!wasFound)
            {
                throw new Exception($"no pack for entity {entityId}");
            }

            return foundPack;
        }
    }
}