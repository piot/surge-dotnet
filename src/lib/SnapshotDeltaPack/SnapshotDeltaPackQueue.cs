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
        private TickId lastInsertedTickId;

        public TickId WantsTickId => new(lastInsertedTickId.tickId + 1);

        public void Enqueue(SnapshotDeltaPack pack)
        {
            if (!IsValidPackToInsert(pack.tickId))
            {
                throw new Exception($"pack can not inserted {pack} {lastInsertedTickId}");
            }

            packs.Enqueue(pack);
            lastInsertedTickId = pack.tickId;
        }

        public SnapshotDeltaPack Dequeue()
        {
            return packs.Dequeue();
        }

        public int Count => packs.Count;

        public bool IsValidPackToInsert(TickId tickId)
        {
            return packs.Count == 0 || tickId.IsImmediateFollowing(lastInsertedTickId);
        }

        public SnapshotDeltaPack Peek()
        {
            return packs.Peek();
        }
    }
}