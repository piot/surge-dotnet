/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Transport.Memory
{
    public static class MemoryTransportFactory
    {
        public static (ITransport, ITransport) CreateClientAndHostTransport()
        {
            var client = new MemoryTransportBoth();
            var host = new MemoryTransportBoth();

            host.SetEnqueueTarget(client, new EndpointId(0));

            client.SetEnqueueTarget(host, new EndpointId(2));

            return (client, host);
        }
    }
}