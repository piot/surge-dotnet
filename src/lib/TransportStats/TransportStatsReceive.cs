/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Stats;
using Piot.Transport;

namespace Piot.Surge.TransportStats
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
            var deltaTimeUntilStats = new Milliseconds(1000);
            bitsPerSecond = new(now, deltaTimeUntilStats, BitFormatter.Format);
            datagramCountPerSecond = new(now, deltaTimeUntilStats);
            datagramOctetSize = new(62);
            wrappedTransport = transportReceive;
        }

        public TransportStatsInDirection Stats => stats;

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            var payload = wrappedTransport.Receive(out remoteEndpointId);
            bitsPerSecond.Add(payload.Length * 8);
            datagramOctetSize.Add(payload.Length);
            datagramCountPerSecond.Add(1);
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