/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Snapshot
{
    /**
     * A snapshot Id. Always increasing by one and consecutive. Usually the session is starting on zero.
     */
    public struct SnapshotId
    {
        public uint frameId;

        public SnapshotId(uint frameId)
        {
            if (frameId == uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(frameId), "max value frame id reserved");
            }

            this.frameId = frameId;
        }

        public bool IsImmediateFollowing(SnapshotId other)
        {
            return other.frameId + 1 == frameId;
        }
        
        public override string ToString()
        {
            return $"{frameId}";
        }
    }
}