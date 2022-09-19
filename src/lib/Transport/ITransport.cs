/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport
{
    public interface ITransportReceive
    {
        public ReadOnlySpan<byte> Receive(out EndpointId endpointId);
    }

    public interface ITransportSend
    {
        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload);
    }

    public interface ITransport : ITransportReceive, ITransportSend
    {
    }

    public interface ITransportEnqueue
    {
        public void Feed(EndpointId endpointId, ReadOnlySpan<byte> payload);
    }

    public readonly struct EndpointId
    {
        public const ushort ReservedForLocalIdValue = ushort.MaxValue;
        public const ushort NoChannelIdValue = 0;
        public static EndpointId NoEndpoint = new(NoChannelIdValue);

        public EndpointId(ushort channel)
        {
            Value = channel;
        }

        public ushort Value { get; }

        public override string ToString()
        {
            return $"[EndpointId {Value}]";
        }
    }
}