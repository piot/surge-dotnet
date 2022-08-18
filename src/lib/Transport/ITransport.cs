/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport
{
    public interface ITransportReceive
    {
        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId);
    }

    public interface ITransportSend
    {
        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload);
    }

    public interface ITransport : ITransportReceive, ITransportSend
    {
        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId);
        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload);
    }

    public struct RemoteEndpointId
    {
        public RemoteEndpointId(uint channel)
        {
            Value = channel;
        }

        public uint Value { get; }
    }
}