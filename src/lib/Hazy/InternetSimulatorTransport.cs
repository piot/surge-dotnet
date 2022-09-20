/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.Transport;

namespace Piot.Hazy
{
    public sealed class InternetSimulatorTransport : ITransport
    {
        readonly InternetSimulatorIn simulatorIn;
        readonly InternetSimulatorOut simulatorOut;

        public InternetSimulatorTransport(ITransport wrapped, IMonotonicTimeMs timeProvider, IRandom random,
            ILog log)
        {
            simulatorIn = new(wrapped, timeProvider, random, log);
            simulatorOut = new(wrapped, timeProvider, random, log);
        }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            return simulatorIn.Receive(out endpointId);
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            simulatorOut.SendToEndpoint(endpointId, payload);
        }

        public void Update()
        {
            simulatorIn.Update();
            simulatorOut.Update();
        }
    }
}