/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class DiscardSendTransport : ITransport
    {
        private readonly ITransportReceive transportReceive;
        
        public DiscardSendTransport(ITransportReceive transportReceive)
        {
            this.transportReceive = transportReceive ?? throw new ArgumentNullException(nameof(transportReceive));
        }
        
        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            return transportReceive.Receive(out endpointId);
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            // Intentionally blank
        }
    }
}