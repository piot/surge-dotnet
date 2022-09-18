/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.IO;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Tick;

namespace Piot.Surge.SnapshotProtocol.Fragment
{
    public sealed class SnapshotFragmentReAssembler
    {
        public enum State
        {
            Receiving,
            Done,
            Discarded
        }

        private readonly ILog log;
        private readonly MemoryStream payloadAssembly = new();
        private TickIdRange assemblingTickIdRange;
        private uint nextDatagramIndex;
        private bool tickIdRangeSet;

        public SnapshotFragmentReAssembler(ILog log)
        {
            this.log = log;
        }

        public State Read(IOctetReader reader, out TickIdRange outTickIdRange, out ReadOnlySpan<byte> outPayload)
        {
            SnapshotFragmentHeaderReader.Read(reader, out var tickIdRange, out var datagramIndex, out var octetCount,
                out var isLastOne);
            log.DebugLowLevel("receive snapshot header {TickIdRange} {DatagramIndex} {IsLastOne}", tickIdRange,
                datagramIndex, isLastOne);
            outTickIdRange = tickIdRange;

            if (!tickIdRangeSet || assemblingTickIdRange != tickIdRange)
            {
                payloadAssembly.SetLength(0);
                if (datagramIndex != 0) // We have caught a snapshot, but not at the start index
                {
                    tickIdRangeSet = false;
                    outPayload = ReadOnlySpan<byte>.Empty;


                    return State.Discarded;
                }

                tickIdRangeSet = true;
                assemblingTickIdRange = tickIdRange;
                nextDatagramIndex = (uint)datagramIndex;
            }

            if (datagramIndex != nextDatagramIndex)
            {
                payloadAssembly.SetLength(0);
                tickIdRangeSet = false;
                outPayload = ReadOnlySpan<byte>.Empty;
                return State.Discarded;
            }

            var fragmentPayload = reader.ReadOctets(octetCount);

            payloadAssembly.Write(fragmentPayload);

            if (isLastOne)
            {
                log.DebugLowLevel("Snapshot fragments are assembled. total {TickIdRange} {OctetCount}",
                    assemblingTickIdRange, fragmentPayload.Length);
            }

            nextDatagramIndex = (uint)datagramIndex + 1;

            outPayload = fragmentPayload;

            return isLastOne ? State.Done : State.Receiving;
        }
    }
}