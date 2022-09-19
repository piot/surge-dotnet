/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public sealed class Packet
    {
        public EndpointId endPoint;
        public TimeMs monotonicTimeMs;
        public ReadOnlyMemory<byte> payload = Array.Empty<byte>();

        public Packet()
        {
        }

        public Packet(TimeMs monotonicTimeMs, EndpointId endPoint, ReadOnlySpan<byte> payload)
        {
            this.monotonicTimeMs = monotonicTimeMs;
            this.endPoint = endPoint;
            this.payload = payload.ToArray();
        }
    }
}