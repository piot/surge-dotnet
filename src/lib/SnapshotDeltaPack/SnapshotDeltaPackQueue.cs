/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    public class SnapshotDeltaPackQueue : ISnapshotDeltaPackQueue
    {
        private readonly Queue<SnapshotDeltaPack> packs = new();
        private SnapshotIdRange lastInsertedRange;

        public void Enqueue(SnapshotDeltaPack pack)
        {
            if (!IsValidPackToInsert(pack.snapshotIdRange))
                throw new Exception($"pack can not inserted {pack} {lastInsertedRange}");

            packs.Enqueue(pack);
            lastInsertedRange = pack.snapshotIdRange;
        }

        public SnapshotDeltaPack Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public bool IsValidPackToInsert(SnapshotIdRange range)
        {
            if (packs.Count == 0) return true;

            return range.IsImmediateFollowing(lastInsertedRange);
        }

        public SnapshotDeltaPack Peek()
        {
            return packs.Peek();
        }
    }
}