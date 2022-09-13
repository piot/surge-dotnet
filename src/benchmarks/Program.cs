using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Piot.Clog;
using Piot.Hazy;
using Piot.MonotonicTime;
using Piot.Random;
using Piot.Surge.Compress;
using Piot.Transport.Memory;

namespace Benchmark.Surge.ExampleGame;

public class BenchmarkGameTest
{
    [Benchmark]
    public void BenchmarkClientAndHostGameUpdate()
    {
        var log = new Log(new ConsoleOutputLogger());
        log.LogLevel = LogLevel.Error;

        var initNow = new Milliseconds(10);

        var (clientTransport, hostTransport) = MemoryTransportFactory.CreateClientAndHostTransport();

        var multiCompressor = DefaultMultiCompressor.Create();
        const bool useInternetSimulation = false;

        var hostTransportToUse = hostTransport;
        InternetSimulatorTransport? internetSimulatedHostTransport = null;

        var timeProvider = new MonotonicTimeMockMs(initNow);
        if (useInternetSimulation)
        {
            var randomizer = new PseudoRandom(0x48019422);
            internetSimulatedHostTransport =
                new InternetSimulatorTransport(hostTransport, timeProvider, randomizer,
                    log.SubLog("InternetSimulator"));
            hostTransportToUse = internetSimulatedHostTransport;
        }

        var clientGame = new BenchmarkGame(clientTransport, multiCompressor, false, log.SubLog("GameClient"));
        var hostGame = new BenchmarkGame(hostTransportToUse, multiCompressor, true, log.SubLog("GameHost"));

        //var world = host.AuthoritativeWorld;
        //var spawnedEntity = world.SpawnEntity(new AvatarLogicEntityInternal());
        //log.Info("Spawned entity {Entity}", spawnedEntity);

        for (var iteration = 0; iteration < 100; iteration++)
        {
            var now = new Milliseconds(20 + iteration * 14);
            timeProvider.TimeInMs = now;
            internetSimulatedHostTransport?.Update();
            clientGame.Update(now);
            hostGame.Update(now);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}