/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Surge.TransportStats
{
    public class TransportStatsBoth : ITransport
    {
        private readonly TransportStatsReceive receive;
        private readonly TransportStatsSend send;

        public TransportStatsBoth(ITransport transport, Milliseconds now)
        {
            receive = new(now, transport);
            send = new(now, transport);
        }

        public TransportStats Stats => new() { receive = receive.Stats, send = send.Stats };

        void ITransportSend.SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            send.SendToEndpoint(remoteEndpointId, payload);
        }

        ReadOnlySpan<byte> ITransportReceive.Receive(out RemoteEndpointId remoteEndpointId)
        {
            return receive.Receive(out remoteEndpointId);
        }

        public void Update(Milliseconds now)
        {
            send.Update(now);
            receive.Update(now);
        }
    }
}