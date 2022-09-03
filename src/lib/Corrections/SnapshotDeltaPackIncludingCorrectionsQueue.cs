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
        private readonly Queue<SnapshotDeltaPackIncludingCorrections> packs = new();
        private TickIdRange lastInsertedTickIdRange;

        private uint wantsTickIdValue = 1;

        public TickId WantsTickId => new(wantsTickIdValue);

        public void Enqueue(SnapshotDeltaPackIncludingCorrections pack)
        {
            if (!IsValidPackToInsert(pack.tickIdRange.Last))
            {
                throw new Exception($"pack can not inserted {pack} {lastInsertedTickIdRange}");
            }

            packs.Enqueue(pack);
            lastInsertedTickIdRange = pack.tickIdRange;
            wantsTickIdValue = lastInsertedTickIdRange.Last.tickId + 1;
        }

        public SnapshotDeltaPackIncludingCorrections Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public bool IsValidPackToInsert(TickId tickId)
        {
            return packs.Count == 0 || tickId.IsImmediateFollowing(lastInsertedTickIdRange.Last);
        }

        public SnapshotDeltaPackIncludingCorrections Peek()
        {
            return packs.Peek();
        }
    }
}