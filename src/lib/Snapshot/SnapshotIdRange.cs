/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Snapshot
{
    /// <summary>
    /// A range of consecutive snapshot IDs.
    /// </summary>
    public struct SnapshotIdRange
    {
        public SnapshotIdRange(SnapshotId containsFromSnapshotId, SnapshotId snapshotId)
        {
            if (containsFromSnapshotId.frameId > snapshotId.frameId)
            {
                throw new ArgumentOutOfRangeException($"snapshotId {containsFromSnapshotId} {snapshotId}");
            }

            this.containsFromSnapshotId = containsFromSnapshotId;
            this.snapshotId = snapshotId;
        }

        public uint Length => snapshotId.frameId - containsFromSnapshotId.frameId + 1;

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