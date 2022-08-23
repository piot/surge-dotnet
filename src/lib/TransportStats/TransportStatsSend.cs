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
            var deltaTimeUntilStats = new Milliseconds(500);
            bitsPerSecond = new(now, deltaTimeUntilStats, BitsPerSecondFormatter.Format);
            datagramCountPerSecond = new(now, deltaTimeUntilStats, StandardFormatterPerSecond.Format);
            datagramOctetSize = new(25);
            wrappedTransport = transportSend;

            stats.bitsPerSecond = bitsPerSecond.Stat;
            stats.datagramCountPerSecond = datagramCountPerSecond.Stat;
            stats.datagramOctetSize = datagramOctetSize.Stat;
        }

        public TransportStatsInDirection Stats => stats;

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            if (payload.Length <= 0)
            {
                return;
            }

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