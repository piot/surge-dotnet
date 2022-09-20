/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport
{
    public sealed class TransportClient : ITransportClient
    {
        readonly ITransport wrappedTransport;

        public TransportClient(ITransport wrappedTransport)
        {
            this.wrappedTransport = wrappedTransport;
        }

        public void SendToHost(ReadOnlySpan<byte> payload)
        {
            wrappedTransport.SendToEndpoint(new(0), payload);
        }

        public ReadOnlySpan<byte> ReceiveFromHost()
        {
            var payload = wrappedTransport.Receive(out var remoteEndpointId);
            if (remoteEndpointId.Value != 0)
            {
                throw new($"should only have a connection with a single host {remoteEndpointId}");
            }

            return payload;
        }
    }
}