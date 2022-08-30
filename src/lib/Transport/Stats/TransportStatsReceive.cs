/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Stats;

namespace Piot.Transport.Stats
{
    public class TransportStatsReceive : ITransportReceive
    {
        private readonly StatPerSecond bitsPerSecond;
        private readonly StatPerSecond datagramCountPerSecond;
        private readonly StatCountThreshold datagramOctetSize;
        private readonly ITransportReceive wrappedTransport;
        private TransportStatsInDirection stats;

        public TransportStatsReceive(Milliseconds now, ITransportReceive transportReceive)
        {
            var deltaTimeUntilStats = new Milliseconds(500);
            bitsPerSecond = new(now, deltaTimeUntilStats, BitsPerSecondFormatter.Format);
            datagramCountPerSecond = new(now, deltaTimeUntilStats, StandardFormatterPerSecond.Format);
            datagramOctetSize = new(25);
            wrappedTransport = transportReceive;
            stats.bitsPerSecond = bitsPerSecond.Stat;
            stats.datagramCountPerSecond = datagramCountPerSecond.Stat;
            stats.datagramOctetSize = datagramOctetSize.Stat;
        }

        public TransportStatsInDirection Stats => stats;

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            var payload = wrappedTransport.Receive(out remoteEndpointId);
            if (payload.Length > 0)
            {
                bitsPerSecond.Add(payload.Length * 8);
                datagramOctetSize.Add(payload.Length);
                datagramCountPerSecond.Add(1);
            }

            return payload;
        }

        public void Update(Milliseconds now)
        {
            bitsPerSecond.Update(now);
            datagramCountPerSecond.Update(now);

            stats.bitsPerSecond = bitsPerSecond.Stat;
            stats.datagramCountPerSecond = datagramCountPerSecond.Stat;
            stats.datagramOctetSize = datagramOctetSize.Stat;
        }
    }
}