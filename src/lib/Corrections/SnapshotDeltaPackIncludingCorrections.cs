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
    public class SnapshotDeltaPackIncludingCorrections
    {
        public ReadOnlyMemory<byte> deltaSnapshotPackPayload;
        public ReadOnlyMemory<byte> physicsCorrections;
        public TickIdRange tickIdRange;

        public SnapshotDeltaPackIncludingCorrections(TickIdRange tickIdRange,
            ReadOnlySpan<byte> deltaSnapshotPackPayload, ReadOnlySpan<byte> physicsCorrections,
            DeltaSnapshotPackType packType)
        {
            this.tickIdRange = tickIdRange;
            this.deltaSnapshotPackPayload = deltaSnapshotPackPayload.ToArray();
            this.physicsCorrections = physicsCorrections.ToArray();
            if (physicsCorrections.Length < 1)
            {
                throw new Exception("invalid physics correction");
            }

            PackType = packType;
        }

        public DeltaSnapshotPackType PackType { get; }

        public override string ToString()
        {
            return
                $"[snapshotDeltaPackIncludingCorrections tickIdRange: {tickIdRange} deltaSnapshotPackPayload length: {deltaSnapshotPackPayload.Length} gamePhysics: {physicsCorrections.Length}]";
        }
    }
}