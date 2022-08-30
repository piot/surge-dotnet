/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Compress;

namespace Piot.Transport.Memory
{
    public static class MemoryTransportFactory
    {
        public static (ITransport, ITransport) CreateClientAndHostTransport()
        {
            var client = new MemoryTransportBoth();
            var host = new MemoryTransportBoth();

            host.SetEnqueueTarget(client, new RemoteEndpointId(0));
            var hostCompressed = new DeflateTransport(host);

            client.SetEnqueueTarget(host, new RemoteEndpointId(2));
            var clientCompressed = new DeflateTransport(client);

            return (clientCompressed, hostCompressed);
        }
    }
}