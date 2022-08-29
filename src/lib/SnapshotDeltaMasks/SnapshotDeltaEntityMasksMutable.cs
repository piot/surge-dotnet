/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.ChangeMask;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaMasks
{
    /// <summary>
    ///     Normally used for building up a <see cref="SnapshotDeltaEntityMasksUnion" />.
    /// </summary>
    public class SnapshotDeltaEntityMasksMutable
    {
        public SnapshotDeltaEntityMasksMutable(TickIdRange tickIdRange)
        {
            TickIdRange = tickIdRange;
        }

        public Dictionary<ushort, ulong> Masks { get; } = new();

        public TickIdRange TickIdRange { get; }

        private ulong Mask(ushort entityIdValue)
        {
            Masks.TryGetValue(entityIdValue, out var mask);
            return mask;
        }

        public void Merge(ushort entityIdValue, ulong afterMask)
        {
            var before = Mask(entityIdValue);
            var mergedBits = ChangedFieldsMerger.MergeBits(before, afterMask);
            Masks[entityIdValue] = mergedBits;
        }

        public void SetChangedMask(EntityId entityId, ulong mask)
        {
            if ((mask & ChangedFieldsMask.DeletedMaskBit) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mask),
                    "use Deleted() method instead to mark entities as deleted");
            }

            SetMask(entityId, mask);
        }

        private void SetMask(EntityId entityId, ulong mask)
        {
            Masks[entityId.Value] = mask;
        }

        public void Deleted(EntityId entityId)
        {
            SetMask(entityId, ChangedFieldsMask.DeletedMaskBit);
        }
    }
}