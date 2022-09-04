/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Tick;

namespace Piot.Surge.Corrections
{
    public class SnapshotDeltaPackIncludingCorrectionsItem
    {
        private readonly TickId? previousTickId;

        public SnapshotDeltaPackIncludingCorrectionsItem(SnapshotDeltaPackIncludingCorrections pack,
            TickId? previousTickId)
        {
            this.previousTickId = previousTickId;
            Pack = pack;
        }

        public SnapshotDeltaPackIncludingCorrections Pack { get; }

        public bool IsMergedAndOverlapping
        {
            get
            {
                if (!previousTickId.HasValue)
                {
                    return false;
                }

                return Pack.tickIdRange.Length > 1 && Pack.tickIdRange.Contains(previousTickId.Value);
            }
        }

        public bool IsSkippedAheadSnapshot
        {
            get
            {
                if (!previousTickId.HasValue)
                {
                    return false;
                }

                return !Pack.tickIdRange.Last.IsImmediateFollowing(previousTickId.Value);
            }
        }
    }
}