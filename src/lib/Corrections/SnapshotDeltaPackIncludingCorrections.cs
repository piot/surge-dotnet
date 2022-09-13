/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;

namespace Piot.Surge.Corrections
{
    /// <summary>
    /// </summary>
    public sealed class SnapshotDeltaPackIncludingCorrections
    {
        public ReadOnlyMemory<byte> deltaSnapshotPackPayload;
        public ReadOnlyMemory<byte> physicsCorrections;
        public TickIdRange tickIdRange;

        public SnapshotDeltaPackIncludingCorrections(TickIdRange tickIdRange,
            ReadOnlySpan<byte> deltaSnapshotPackPayload, ReadOnlySpan<byte> physicsCorrections,
            SnapshotStreamType streamType, SnapshotType snapshotType)
        {
            this.tickIdRange = tickIdRange;
            this.deltaSnapshotPackPayload = deltaSnapshotPackPayload.ToArray();
            this.physicsCorrections = physicsCorrections.ToArray();
            if (physicsCorrections.Length < 1)
            {
                throw new Exception("invalid physics correction");
            }

            StreamType = streamType;
            SnapshotType = snapshotType;
        }

        public SnapshotStreamType StreamType { get; }
        public SnapshotType SnapshotType { get; }

        public override string ToString()
        {
            return
                $"[snapshotDeltaPackIncludingCorrections tickIdRange: {tickIdRange} deltaSnapshotPackPayload length: {deltaSnapshotPackPayload.Length} gamePhysics: {physicsCorrections.Length} type:{SnapshotType} stream:{StreamType}]";
        }
    }
}