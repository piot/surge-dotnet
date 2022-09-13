/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport.Memory
{
    public sealed class MemoryTransportBoth : ITransport, ITransportEnqueue
    {
        private readonly MemoryTransportReceive receive = new();
        private ITransportEnqueue? enqueueTarget;
        private RemoteEndpointId knownAsOnReceiver;

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            return receive.Receive(out remoteEndpointId);
        }

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            if (enqueueTarget is null)
            {
                throw new Exception("EnqueueTarget is not set");
            }

            enqueueTarget.Feed(knownAsOnReceiver, payload);
        }

        public void Feed(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            receive.Feed(remoteEndpointId, payload);
        }

        public void SetEnqueueTarget(ITransportEnqueue enqueueTarget, RemoteEndpointId knownAsOnReceiver)
        {
            this.enqueueTarget = enqueueTarget;
            this.knownAsOnReceiver = knownAsOnReceiver;
        }
    }
}