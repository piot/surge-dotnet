/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Net;

namespace Piot.UdpTransport
{
    public class UdpTransportClient : IUdpTransportClient
    {
        private readonly IPEndPoint serverEndpoint;
        private readonly IUdpTransportSocket socket;
        
        public UdpTransportClient(IPEndPoint serverEndpoint, IUdpTransportSocket socket)
        {
            this.serverEndpoint = serverEndpoint;
            this.socket = socket;
        }
        public void Send(ReadOnlySpan<byte> octets)
        {
            socket.Send(octets, serverEndpoint);
        }

        public ReadOnlySpan<byte> Receive()
        {
            var received = socket.Receive(out var encounteredRemoteEndpoint);
            return (encounteredRemoteEndpoint.Address != serverEndpoint.Address || encounteredRemoteEndpoint.Port != serverEndpoint.Port) ? new ReadOnlySpan<byte>() : received;
        }
    }
}