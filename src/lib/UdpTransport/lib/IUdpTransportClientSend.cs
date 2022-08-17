/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.UdpTransport
{
    public interface IUdpTransportClientSend
    {
        public void Send(ReadOnlySpan<byte> octets);
    }
}