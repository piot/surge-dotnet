/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaMasks
{
    /// <summary>
    ///     <see cref="ChangedFieldsMask" /> for all the entities that changed this tick.
    /// </summary>
    public class SnapshotDeltaEntityMasks
    {
        public SnapshotDeltaEntityMasks(TickId tickId, Dictionary<ushort, ulong> entityToMask)
        {
            TickId = tickId;
            EntityMasks = entityToMask;
        }

        public SnapshotDeltaEntityMasks(SnapshotDeltaEntityMasksMutable mutable)
        {
            TickId = mutable.TickIdRange.Last;
            EntityMasks = mutable.Masks;
        }

        public Dictionary<ushort, ulong> EntityMasks { get; }

        public TickId TickId { get; }

        public ulong FetchEntity(EntityId entityId)
        {
            return EntityMasks[entityId.Value];
        }
    }
}