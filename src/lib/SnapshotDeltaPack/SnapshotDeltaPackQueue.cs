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
        private SnapshotId lastInsertedSnapshotId;

        public void Enqueue(SnapshotDeltaPack pack)
        {
            if (!IsValidPackToInsert(pack.snapshotId))
                throw new Exception($"pack can not inserted {pack} {lastInsertedSnapshotId}");

            packs.Enqueue(pack);
            lastInsertedSnapshotId = pack.snapshotId;
        }

        public SnapshotDeltaPack Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public bool IsValidPackToInsert(SnapshotId range)
        {
            return packs.Count == 0 || range.IsImmediateFollowing(lastInsertedSnapshotId);
        }

        public SnapshotDeltaPack Peek()
        {
            return packs.Peek();
        }
    }
}