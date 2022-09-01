/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.SnapshotProtocol
{
    /// <summary>
    /// </summary>
    public class SnapshotProtocolPack
    {
        public ReadOnlyMemory<byte> payload;
        public TickIdRange tickIdRange;

        public SnapshotProtocolPack(TickIdRange tickIdRange, ReadOnlySpan<byte> payload)
        {
            this.payload = payload.ToArray();
            this.tickIdRange = tickIdRange;
        }

        public TickIdRange TickIdRange => tickIdRange;

        public override string ToString()
        {
            return
                $"[SnapshotProtocolPack deltaSnapshotPackPayload length: {payload.Length}]";
        }
    }
}