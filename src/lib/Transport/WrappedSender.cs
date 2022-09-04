/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport
{
    public class WrappedSender
    {
        private readonly RemoteEndpointId id;
        private readonly ITransportSend sender;

        public WrappedSender(ITransportSend sender, RemoteEndpointId id)
        {
            this.sender = sender;
            this.id = id;
        }

        public void Send(ReadOnlySpan<byte> datagram)
        {
            sender.SendToEndpoint(id, datagram);
        }
    }
}