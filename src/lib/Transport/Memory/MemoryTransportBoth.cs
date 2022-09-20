/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Transport.Memory
{
    public sealed class MemoryTransportBoth : ITransport, ITransportEnqueue
    {
        readonly MemoryTransportReceive receive = new();
        ITransportEnqueue? enqueueTarget;
        EndpointId knownAsOnReceiver;

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            return receive.Receive(out endpointId);
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            if (enqueueTarget is null)
            {
                throw new("EnqueueTarget is not set");
            }

            enqueueTarget.Feed(knownAsOnReceiver, payload);
        }

        public void Feed(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            receive.Feed(endpointId, payload);
        }

        public void SetEnqueueTarget(ITransportEnqueue enqueueTarget, EndpointId knownAsOnReceiver)
        {
            this.enqueueTarget = enqueueTarget;
            this.knownAsOnReceiver = knownAsOnReceiver;
        }
    }
}