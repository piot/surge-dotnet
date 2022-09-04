/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Pulse.Client
{
    public struct ClientNetworkQuality
    {
        /// <summary>
        ///     Is true if a snapshot for a tick has been dropped, and we skipped to a newer snapshot state.
        ///     If true it will result in considerably lower playback quality, since certain transitions will not be noticed and
        ///     triggered.
        ///     e.g. missing if someone pressed fire on a snapshot that was skipped/dropped. It is almost like a minor
        ///     reconnect/rejoin, but using the
        ///     snapshot tickId that the host knows that we received previously.
        /// </summary>
        public bool isSkippingSnapshots;

        /// <summary>
        ///     If merged snapshots are received or have been received recently. Merged snapshots comprise of a merge state of
        ///     multiple states in a tick range.
        ///     It should not impact the perceived quality other than the redundancy is taking up a little bit more bandwidth.
        /// </summary>
        public bool isReceivingMergedSnapshots;

        /// <summary>
        ///     the incoming snapshot buffer size is getting low, so we can not interpolate, but are forced to extrapolate.
        /// </summary>
        public bool isIncomingSnapshotPlaybackBufferStarving;

        /// <summary>
        ///     average round trip time in milliseconds.
        /// </summary>
        public uint averageRoundTripTimeMs;

        public override string ToString()
        {
            return
                $"[ClientNetQuality RTT:{averageRoundTripTimeMs} skipping:{isSkippingSnapshots} snapshotMerged:{isReceivingMergedSnapshots} bufferStarving:{isIncomingSnapshotPlaybackBufferStarving} ]";
        }
    }
}