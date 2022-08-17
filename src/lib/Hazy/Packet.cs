/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class Packet
    {
        public Milliseconds monotonicTimeMs;
        public byte[] payload = Array.Empty<byte>();
        public RemoteEndpointId endPoint;
    }
}