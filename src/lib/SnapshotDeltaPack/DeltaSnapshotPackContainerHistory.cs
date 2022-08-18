/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    public class DeltaSnapshotPackContainerHistory
    {
        private readonly Queue<DeltaSnapshotPackContainer> queue = new();
        private TickIdRange tickIdRange;

        public void Add(DeltaSnapshotPackContainer container)
        {
            var tickId = container.TickId;
            if (queue.Count > 0 && !tickId.IsImmediateFollowing(tickIdRange.Last))
            {
                throw new ArgumentOutOfRangeException(nameof(tickId));
            }

            queue.Enqueue(container);
            tickIdRange = new TickIdRange(tickIdRange.startTickId, tickIdRange.Last);
        }

        public DeltaSnapshotPackContainer[] FetchContainers(TickIdRange queryIdRange)
        {
            if (!tickIdRange.Contains(queryIdRange))
            {
                throw new ArgumentOutOfRangeException(nameof(queryIdRange));
            }

            var (startOffset, endOffset) = tickIdRange.Offsets(queryIdRange);

            var sourceArray = queue.ToArray();

            var targetContainers = new DeltaSnapshotPackContainer[endOffset - startOffset + 1];
            for (var i = startOffset; i <= endOffset; ++i)
            {
                var source = sourceArray[i];
                targetContainers[i - startOffset] = source;
            }

            return targetContainers;
        }
    }
}