/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.Surge.Internal.Generated;
using Piot.Surge.LogicalInput;
using Piot.Surge.MemoryTransport;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Pulse.Host;
using Piot.Transport;
using Xunit.Abstractions;

namespace Tests.Pulse;

public class MockInputFetch : IInputPackFetch
{
    public Span<byte> Fetch()
    {
        return new byte[] { 0xca, 0xfe };
    }
}

public class ClientHostTests
{
    private readonly ILog log;
    private readonly TestOutputLogger logTarget;

    public ClientHostTests(ITestOutputHelper output)
    {
        logTarget = new TestOutputLogger(output);
        log = new Log(logTarget, LogLevel.LowLevel);
    }

    private Client CreateClient(Milliseconds now, ITransport transport)
    {
        var clientDeltaTime = new Milliseconds(16);
        var inputFetch = new MockInputFetch();
        var client = new Client(log.SubLog("Client"), now, clientDeltaTime, new GeneratedEntityCreation(),
            transport, inputFetch);

        return client;
    }

    private Host CreateHost(Milliseconds now, ITransport transport)
    {
        var host = new Host(transport, now, log.SubLog("Host"));
        return host;
    }

    [Fact]
    public void TestClientAndHostUpdates()
    {
        var initNow = new Milliseconds(10);

        var (clientTransport, hostTransport) = MemoryTransportFactory.CreateClientAndHostTransport();

        var client = CreateClient(initNow, clientTransport);
        var host = CreateHost(initNow, hostTransport);

        for (var iteration = 0; iteration < 62; iteration++)
        {
            var now = new Milliseconds(20 + iteration * 14);
            client.Update(now);
            host.Update(now);
        }
    }
}