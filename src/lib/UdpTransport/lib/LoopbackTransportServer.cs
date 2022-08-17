/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Net;

namespace Piot.UdpTransport
{
    public class LoopbackTransportSocket : IUdpTransportSocket, IUdpTransportFeed
    {
        readonly Queue<byte[]> inQueue = new();
        private IUdpTransportFeed client;

        public LoopbackTransportSocket(IUdpTransportFeed feed)
        {
            client = feed;
        }
        
        public IUdpTransportFeed Target
        {
            set => client = value;
        }

        public void Send(ReadOnlySpan<byte> octets, IPEndPoint remoteEndpoint)
        {
            if (remoteEndpoint.Address != IPAddress.Loopback)
            {
                throw new Exception($"must be loopback address to send to loopback transport");
            }
            client.Feed(octets);
        }

        public ReadOnlySpan<byte> Receive(out IPEndPoint remoteEndpoint)
        {
            remoteEndpoint = new IPEndPoint(IPAddress.Loopback, 0);
            return inQueue.Count == 0 ? new ReadOnlySpan<byte>() : inQueue.Dequeue();
        }

        void IUdpTransportFeed.Feed(ReadOnlySpan<byte> octets)
        {
            inQueue.Enqueue(octets.ToArray());   
        }
    }
}