/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Snapshot
{
    public struct SnapshotIdRange
    {
        public SnapshotIdRange(SnapshotId containsFromSnapshotId, SnapshotId snapshotId)
        {
            if (containsFromSnapshotId.frameId > snapshotId.frameId)
                throw new ArgumentOutOfRangeException($"snapshotId {containsFromSnapshotId} {snapshotId}");
            this.containsFromSnapshotId = containsFromSnapshotId;
            this.snapshotId = snapshotId;
        }

        public bool IsImmediateFollowing(SnapshotIdRange other)
        {
            return other.snapshotId.frameId + 1 == containsFromSnapshotId.frameId;
        }

        public override string ToString()
        {
            return $"[snapshotIdRange {containsFromSnapshotId} {snapshotId}]";
        }

        public SnapshotId snapshotId;
        public SnapshotId containsFromSnapshotId;
    }
}