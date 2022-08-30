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
    public class SnapshotFragmentReAssembler
    {
        private readonly MemoryStream payloadAssembly = new();
        private bool tickIdRangeSet;
        private TickIdRange assemblingTickIdRange;
        private uint nextDatagramIndex;
        private readonly ILog log;
        
        public SnapshotFragmentReAssembler(ILog log)
        {
            this.log = log;
        }
        
        public bool Read(IOctetReader reader, out TickIdRange outTickIdRange, out ReadOnlySpan<byte> outPayload)
        {
            SnapshotFragmentHeaderReader.Read(reader, out var tickIdRange, out var datagramIndex, out var octetCount, out var isLastOne);
            log.DebugLowLevel("receive snapshot header {TickIdRange} {DatagramIndex} {IsLastOne}", tickIdRange,
                datagramIndex, isLastOne);

            if (!tickIdRangeSet || assemblingTickIdRange != tickIdRange)
            {
                payloadAssembly.SetLength(0);
                if (datagramIndex != 0)
                {
                    tickIdRangeSet = false;
                    outPayload = ReadOnlySpan<byte>.Empty;
                    outTickIdRange = tickIdRange;
                    return false;
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
                outTickIdRange = tickIdRange;
                return false;
            }

            var fragmentPayload = reader.ReadOctets(octetCount);

            payloadAssembly.Write(fragmentPayload);

            if (isLastOne)
            {
                log.DebugLowLevel("Snapshot fragments are assembled. total {TickIdRange} {OctetCount}", assemblingTickIdRange, fragmentPayload.Length);
            }

            nextDatagramIndex = (uint)datagramIndex + 1;
            
            outPayload = fragmentPayload;
            outTickIdRange = tickIdRange;
            return isLastOne;
        }
    }
}