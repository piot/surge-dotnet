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
    public sealed class InternetSimulatorTransport : IUpdateTransport
    {

        public InternetSimulatorTransport(ITransport wrapped, IMonotonicTimeMs timeProvider, IRandom random,
            ILog log)
        {
            In = new(wrapped, timeProvider, random, log);
            Out = new(wrapped, timeProvider, random, log);
        }

        public InternetSimulatorIn In { get; }

        public InternetSimulatorOut Out { get; }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            return In.Receive(out endpointId);
        }

        public void SendToEndpoint(EndpointId endpointId, ReadOnlySpan<byte> payload)
        {
            Out.SendToEndpoint(endpointId, payload);
        }

        public void Update()
        {
            In.Update();
            Out.Update();
        }
    }
}