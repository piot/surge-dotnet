/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    ///     Holds a queue of previous delta snapshot packs. Used primarily when connection indicates
    ///     that they want a delta snapshot to be resent.
    /// </summary>
    public class DeltaSnapshotPackContainerHistory
    {
        private readonly ILog log;
        private readonly Queue<DeltaSnapshotPackContainer> queue = new();
        private TickIdRange tickIdRange;

        public DeltaSnapshotPackContainerHistory(ILog log)
        {
            this.log = log;
        }

        public void Add(DeltaSnapshotPackContainer container)
        {
            var tickId = container.TickId;
            if (queue.Count > 0 && !tickId.IsImmediateFollowing(tickIdRange.Last))
            {
                throw new ArgumentOutOfRangeException(nameof(tickId),
                    $"was expecting snapshot pack container following {tickIdRange.Last}, but was {tickId}");
            }

            log.DebugLowLevel("Adding snapshot container {Container}", container);

            queue.Enqueue(container);
            tickIdRange = new TickIdRange(tickIdRange.startTickId, tickId);
        }

        /// <summary>
        ///     Returns a collection of containers for the specified <paramref name="queryIdRange" />.
        /// </summary>
        /// <param name="queryIdRange"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DeltaSnapshotPackContainer[] FetchContainers(TickIdRange queryIdRange)
        {
            if (!tickIdRange.Contains(queryIdRange))
            {
                throw new ArgumentOutOfRangeException(nameof(queryIdRange));
            }

            var (startOffset, endOffset) = tickIdRange.Offsets(queryIdRange);

            var sourceArray = queue.ToArray();
            log.DebugLowLevel("range {StartOffset}, {EndOffset}", startOffset, endOffset);

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