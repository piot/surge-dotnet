/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Net;
using System.Net.Sockets;
using Piot.Clog;
using Piot.Transport;

namespace Piot.UdpServer
{
    public class Client : ITransport
    {
        readonly byte[] octetsArray = new byte[1200];
        readonly EndPoint serverEndPoint;
        readonly Socket socket;
        readonly ILog log;

        public Client(string hostname, ushort listenPort, ILog log)
        {
            this.log = log;
            var hostEntry = Dns.GetHostByName(hostname);
            serverEndPoint = new IPEndPoint(hostEntry.AddressList[0], listenPort);

            //var bindEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var bindEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);

            socket = new(hostEntry.AddressList[0].AddressFamily,
                SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);

            socket.Blocking = false;

            try
            {
                socket.Bind(bindEndPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            //var foundPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
            var castFoundPoint = serverEndPoint;

            var octetCountReceived = 0;
            try
            {
                octetCountReceived = socket.ReceiveFrom(octetsArray, ref castFoundPoint);
                if (octetCountReceived == 0)
                {
                    endpointId = new(0);
                    return ReadOnlySpan<byte>.Empty;
                }

                log.DebugLowLevel("received {Length} from {Endpoint}", octetCountReceived, castFoundPoint);
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

            if (!castFoundPoint.Equals(serverEndPoint))
            {
                endpointId = new(0);
                return ReadOnlySpan<byte>.Empty;
            }

            endpointId = new(0);

            return octetsArray.AsSpan().Slice(0, octetCountReceived);
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            if (payload.IsEmpty)
            {
                return;
            }

            var sentOctets = socket.SendTo(payload.ToArray(), serverEndPoint);
            if (sentOctets != payload.Length)
            {
                throw new("could not send");
            }

            log.DebugLowLevel("sent {Length} to {Endpoint}", payload.Length, serverEndPoint);
        }
    }
}