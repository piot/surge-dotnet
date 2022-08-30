/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Corrections
{
    /// <summary>
    /// </summary>
    public class SnapshotDeltaPackIncludingCorrections
    {
        public ReadOnlyMemory<byte> payload;
        public TickIdRange tickIdRange;

        public SnapshotDeltaPackIncludingCorrections(TickIdRange tickIdRange, ReadOnlySpan<byte> payload)
        {
            this.tickIdRange = tickIdRange;
            this.payload = payload.ToArray();
        }

        public override string ToString()
        {
            return
                $"[snapshotDeltaPackIncludingCorrections tickIdRange: {tickIdRange} payload length: {payload.Length}]";
        }
    }
}