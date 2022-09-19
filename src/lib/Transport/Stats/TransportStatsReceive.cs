/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Collections;
using Piot.MonotonicTime;
using Piot.Stats;

namespace Piot.Transport.Stats
{
    public sealed class TransportStatsReceive : ITransportReceive
    {
        private readonly StatPerSecond bitsPerSecond;
        private readonly StatPerSecond datagramCountPerSecond;
        private readonly StatCountThreshold datagramOctetSize;
        private readonly CircularBuffer<int> datagramOctetSizes = new(Constants.CircularBufferSize);
        private readonly ITransportReceive wrappedTransport;

        private TransportStatsInDirection stats;

        public TransportStatsReceive(TimeMs now, ITransportReceive transportReceive)
        {
            var deltaTimeUntilStats = new TimeMs(500);
            bitsPerSecond = new(now, deltaTimeUntilStats, BitsPerSecondFormatter.Format);
            datagramCountPerSecond = new(now, deltaTimeUntilStats, StandardFormatterPerSecond.Format);
            datagramOctetSize = new(25);
            wrappedTransport = transportReceive;
            stats.bitsPerSecond = bitsPerSecond.Stat;
            stats.datagramCountPerSecond = datagramCountPerSecond.Stat;
            stats.datagramOctetSize = datagramOctetSize.Stat;
            stats.datagramOctetSizes = datagramOctetSizes;
        }

        public TransportStatsInDirection Stats => stats;

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            var payload = wrappedTransport.Receive(out endpointId);
            if (payload.Length > 0)
            {
                bitsPerSecond.Add(payload.Length * 8);
                datagramOctetSize.Add(payload.Length);
                datagramOctetSizes.Enqueue(payload.Length);
                datagramCountPerSecond.Add(1);
            }

            return payload;
        }

        public void Update(TimeMs now)
        {
            bitsPerSecond.Update(now);
            datagramCountPerSecond.Update(now);

            stats.bitsPerSecond = bitsPerSecond.Stat;
            stats.datagramCountPerSecond = datagramCountPerSecond.Stat;
            stats.datagramOctetSize = datagramOctetSize.Stat;
        }
    }
}