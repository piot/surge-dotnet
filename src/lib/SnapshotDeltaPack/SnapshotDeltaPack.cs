/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    public class SnapshotDeltaPack
    {
        public Memory<byte> payload;
        public SnapshotIdRange snapshotIdRange;

        public SnapshotDeltaPack(SnapshotIdRange range, Memory<byte> payload)
        {
            snapshotIdRange = range;
            this.payload = payload;
        }

        public override string ToString()
        {
            return $"[snapshotDeltaPack {snapshotIdRange} {payload}]";
        }
    }
}