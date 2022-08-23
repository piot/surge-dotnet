/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Transport;

namespace Piot.Surge.MemoryTransport
{
    public static class MemoryTransportFactory
    {
        public static (ITransport, ITransport) CreateClientAndHostTransport()
        {
            var client = new MemoryTransportBoth();
            var host = new MemoryTransportBoth();

            host.SetEnqueueTarget(client, new RemoteEndpointId(0));
            client.SetEnqueueTarget(host, new RemoteEndpointId(2));

            return (client, host);
        }
    }
}