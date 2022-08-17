/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Net;

namespace Piot.UdpTransport
{
    public interface IUdpTransportSocketSend
    {
        public void Send(ReadOnlySpan<byte> octets, IPEndPoint remoteEndpoint);
    }
}