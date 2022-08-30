/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Snapshot;

namespace Piot.Surge.DeltaSnapshot.Cache
{
    /// <summary>
    ///     Holds a queue of previous delta snapshot packs. Used primarily when connection indicates
    ///     that they want a delta snapshot to be resent.
    /// </summary>
    public class DeltaSnapshotPackCache
    {
        private readonly ILog log;
        private readonly Queue<DeltaSnapshotPack> queue = new();
        private TickIdRange tickIdRange;

        public DeltaSnapshotPackCache(ILog log)
        {
            this.log = log;
        }

        public void Add(DeltaSnapshotPack container)
        {
            var tickId = container.TickIdRange.Last;
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
        ///     Returns a snapshot pack for the specified <paramref name="queryIdRange" />.
        /// </summary>
        /// <param name="queryIdRange"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool FetchPack(TickIdRange queryIdRange, out DeltaSnapshotPack outPack)
        {
            throw new NotImplementedException();
        }
    }
}