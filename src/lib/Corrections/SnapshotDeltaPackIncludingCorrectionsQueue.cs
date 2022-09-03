/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.Corrections
{
    public class SnapshotDeltaPackIncludingCorrectionsItem
    {
        private readonly TickId? previousTickId;

        public SnapshotDeltaPackIncludingCorrectionsItem(SnapshotDeltaPackIncludingCorrections pack,
            TickId? previousTickId)
        {
            this.previousTickId = previousTickId;
            Pack = pack;
        }

        public SnapshotDeltaPackIncludingCorrections Pack { get; }

        public bool IsMergedAndOverlapping
        {
            get
            {
                if (!previousTickId.HasValue)
                {
                    return false;
                }

                return Pack.tickIdRange.Length > 1 && Pack.tickIdRange.Contains((TickId)previousTickId);
            }
        }
    }

    public class SnapshotDeltaPackIncludingCorrectionsQueue : ISnapshotDeltaPackQueue
    {
        private readonly Queue<SnapshotDeltaPackIncludingCorrectionsItem> packs = new();
        private TickIdRange lastInsertedTickIdRange;

        private uint wantsTickIdValue = 1;

        public TickId WantsTickId => new(wantsTickIdValue);

        public bool LastInsertedIsMergedSnapshot => packs.Count > 0 && lastInsertedTickIdRange.Length > 1;
        public TickIdRange LastInsertedTickIdRange => lastInsertedTickIdRange;

        public void Enqueue(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!IsValidPackToInsert(pack.tickIdRange.Last))
            {
                throw new Exception($"pack can not inserted {pack} {lastInsertedTickIdRange}");
            }

            var lastInsertedTickId = packs.Count > 0 ? lastInsertedTickIdRange.Last : (TickId?)null;

            packs.Enqueue(new(pack, lastInsertedTickId));
            lastInsertedTickIdRange = pack.tickIdRange;
            wantsTickIdValue = lastInsertedTickIdRange.Last.tickId + 1;
        }

        public SnapshotDeltaPackIncludingCorrectionsItem Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public bool IsValidPackToInsert(TickId tickId)
        {
            return packs.Count == 0 || tickId > lastInsertedTickIdRange.Last;
        }

        public SnapshotDeltaPackIncludingCorrectionsItem Peek()
        {
            return packs.Peek();
        }
    }
}