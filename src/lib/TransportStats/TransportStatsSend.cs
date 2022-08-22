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
    public class TransportStatsSend : ITransportSend
    {
        private readonly StatPerSecond bitsPerSecond;
        private readonly StatPerSecond datagramCountPerSecond;
        private readonly StatCountThreshold datagramOctetSize;
        private readonly ITransportSend wrappedTransport;
        private TransportStatsInDirection stats;

        public TransportStatsSend(Milliseconds now, ITransportSend transportSend)
        {
            var deltaTimeUntilStats = new Milliseconds(1000);
            bitsPerSecond = new StatPerSecond(now, deltaTimeUntilStats, BitFormatter.Format);
            datagramCountPerSecond = new StatPerSecond(now, deltaTimeUntilStats);
            datagramOctetSize = new StatCountThreshold(62);
            wrappedTransport = transportSend;
        }

        public TransportStatsInDirection Stats => stats;

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            bitsPerSecond.Add(payload.Length * 8);
            datagramOctetSize.Add(payload.Length);
            datagramCountPerSecond.Add(1);
            wrappedTransport.SendToEndpoint(remoteEndpointId, payload);
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