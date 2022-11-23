/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Piot.Clog;
using Piot.Transport;

namespace Piot.UdpServer
{
    public class Server : ITransport
    {
        readonly Dictionary<EndPoint, ushort> endpointToInfo = new();
        readonly Dictionary<ushort, EndPoint> infoToEndpoint = new();
        readonly ILog log;
        readonly byte[] octetsArray = new byte[1200];
        readonly Socket socket;
        ushort connectionId;

        public Server(ushort listenPort, ILog log)
        {
            this.log = log;
            var localEndPoint = new IPEndPoint(IPAddress.IPv6Any, listenPort);

            socket = new(AddressFamily.InterNetworkV6, // Ipv4 InterNetwork  
                SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

            socket.Blocking = false;

            try
            {
                socket.Bind(localEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            var foundPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
            var castFoundPoint = (EndPoint)foundPoint;

            var octetCountReceived = 0;
            try
            {
                octetCountReceived = socket.ReceiveFrom(octetsArray, ref castFoundPoint);
                if (octetCountReceived == 0)
                {
                    endpointId = new(0);
                    return ReadOnlySpan<byte>.Empty;
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.WouldBlock)
                {
                    endpointId = new(0);
                    return ReadOnlySpan<byte>.Empty;
                }

                Console.WriteLine(e);
                throw;
            }


            var found = endpointToInfo.ContainsKey(castFoundPoint);
            ushort foundConnectionId;
            if (found)
            {
                foundConnectionId = endpointToInfo[castFoundPoint];
            }
            else
            {
                ++connectionId;
                endpointToInfo.Add(castFoundPoint, connectionId);
                infoToEndpoint.Add(connectionId, castFoundPoint);
                foundConnectionId = connectionId;
                log.DebugLowLevel("Created connection {ConnectionId} {castFoundPoint}", connectionId, castFoundPoint);
            }

            endpointId = new(foundConnectionId);

            return octetsArray.AsSpan()[..octetCountReceived];
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            if (!infoToEndpoint.ContainsKey(endpointId.Value))
            {
                throw new($"connection id {endpointId} is not known");
            }

            var existingEndpoint = infoToEndpoint[endpointId.Value];
            log.DebugLowLevel("Sending to {ConnectionId} {existingEndpoint}", endpointId, existingEndpoint);
            socket.SendTo(payload.ToArray(), existingEndpoint);
        }
    }
}