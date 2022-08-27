/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Transport;

namespace Piot.Surge.Compress
{
    public class DeflateTransport : ITransport
    {
        private readonly ITransport wrappedTransport;

        public DeflateTransport(ITransport wrappedTransport)
        {
            this.wrappedTransport = wrappedTransport;
        }

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            var octets = wrappedTransport.Receive(out remoteEndpointId);
            return Deflate.Decompress(octets);
        }

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            var compressed = Deflate.Compress(payload);
            wrappedTransport.SendToEndpoint(remoteEndpointId, compressed);
        }
    }
}