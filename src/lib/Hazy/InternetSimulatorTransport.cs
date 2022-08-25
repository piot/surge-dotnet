/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public class InternetSimulatorTransport : ITransport
    {
        private readonly InternetSimulatorIn simulatorIn;
        private readonly InternetSimulatorOut simulatorOut;

        public InternetSimulatorTransport(ITransport wrapped, IMonotonicTimeMs timeProvider, IRandom random,
            ILog log)
        {
            simulatorIn = new(wrapped, timeProvider, random, log);
            simulatorOut = new(wrapped, timeProvider, random, log);
        }

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            return simulatorIn.Receive(out remoteEndpointId);
        }

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> payload)
        {
            simulatorOut.SendToEndpoint(remoteEndpointId, payload);
        }

        public void Update()
        {
            simulatorIn.Update();
            simulatorOut.Update();
        }
    }
}