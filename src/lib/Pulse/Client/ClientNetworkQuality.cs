/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Pulse.Client
{
    public struct ClientNetworkQuality
    {
        public bool isSkippingSnapshots;
        public bool isReceivingMergedSnapshots;
        public bool isIncomingSnapshotPlaybackBufferStarving;
        public uint averageRoundTripTimeMs;

        public override string ToString()
        {
            return
                $"[ClientNetQuality RTT:{averageRoundTripTimeMs} skipping:{isSkippingSnapshots} snapshotMerged:{isReceivingMergedSnapshots} bufferStarving:{isIncomingSnapshotPlaybackBufferStarving} ]";
        }
    }
}