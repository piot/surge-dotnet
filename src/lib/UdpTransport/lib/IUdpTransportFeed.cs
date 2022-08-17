/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.UdpTransport
{
    public interface IUdpTransportFeed
    {
        void Feed(ReadOnlySpan<byte> octets);
    }
}