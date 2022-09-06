/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.Corrections
{
    public class SnapshotDeltaPackIncludingCorrectionsQueue : ISnapshotDeltaPackQueue
    {
        private readonly Queue<SnapshotDeltaPackIncludingCorrectionsItem> packs = new();

        private bool hasBeenInitialized;
        private TickIdRange lastInsertedTickIdRange;

        private uint wantsTickIdValue = 1;

        public TickId WantsTickId => new(wantsTickIdValue);

        public bool LastInsertedIsMergedSnapshot => hasBeenInitialized && lastInsertedTickIdRange.Length > 1;
        public TickIdRange LastInsertedTickIdRange => lastInsertedTickIdRange;

        public void Enqueue(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!IsValidPackToInsert(pack.tickIdRange))
            {
                throw new Exception($"pack can not inserted {pack} {lastInsertedTickIdRange}");
            }


            hasBeenInitialized = true;

            var lastInsertedTickId = packs.Count > 0 ? lastInsertedTickIdRange.Last : (TickId?)null;

            packs.Enqueue(new(pack, lastInsertedTickId));
            lastInsertedTickIdRange = pack.tickIdRange;
            wantsTickIdValue = lastInsertedTickIdRange.Last.Next().tickId;
        }

        public SnapshotDeltaPackIncludingCorrectionsItem Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public int TicksAheadOf(TickId tickId)
        {
            if (packs.Count == 0)
            {
                return 0;
            }

            return (int)(Peek().Pack.tickIdRange.Last.tickId - tickId.tickId);
        }

        public bool IsValidPackToInsert(TickIdRange tickIdRange)
        {
            return !hasBeenInitialized || lastInsertedTickIdRange.CanAppend(tickIdRange);
        }

        public SnapshotDeltaPackIncludingCorrectionsItem Peek()
        {
            return packs.Peek();
        }
    }
}