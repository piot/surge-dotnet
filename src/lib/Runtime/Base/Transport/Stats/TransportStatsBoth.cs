/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;

namespace Piot.Transport.Stats
{
    public sealed class TransportStatsBoth : ITransport
    {
        readonly TransportStatsReceive receive;
        readonly TransportStatsSend send;
        ITransport bothTransport;

        public TransportStatsBoth(ITransport transport, TimeMs now)
        {
            receive = new(now, transport);
            send = new(now, transport);
            bothTransport = transport;
        }

        public TransportStats Stats => new()
        {
            receive = receive.Stats, send = send.Stats
        };

        public ITransport Transport
        {
            get => bothTransport;
            set
            {
                bothTransport = value;
                receive.WrappedTransport = value;
                send.WrappedTransport = value;
            }
        }

        void ITransportSend.SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            send.SendToEndpoint(endpointId, payload);
        }

        ReadOnlySpan<byte> ITransportReceive.Receive(out EndpointId endpointId)
        {
            return receive.Receive(out endpointId);
        }

        public void Update(TimeMs now)
        {
            send.Update(now);
            receive.Update(now);
        }
    }
}