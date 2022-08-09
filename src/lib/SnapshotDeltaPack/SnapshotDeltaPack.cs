/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    /// Holds a complete delta snapshot serialized pack payload.
    /// </summary>
    public class SnapshotDeltaPack
    {
        public Memory<byte> payload;
        public SnapshotId snapshotId;

        public SnapshotDeltaPack(SnapshotId snapshotId, Memory<byte> payload)
        {
            this.snapshotId = snapshotId;
            this.payload = payload;
        }

        public override string ToString()
        {
            return $"[snapshotDeltaPack {snapshotId} {payload}]";
        }
    }
}