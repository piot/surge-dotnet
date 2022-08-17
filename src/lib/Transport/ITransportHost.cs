using System;

namespace Piot.Surge.Transport
{
    public interface ITransportHost
    {
        public Memory<byte> Receive(out ClientId clientId);
        public void SendToClient(ClientId clientId, Memory<byte> payload);
    }

    public struct ClientId
    {
        public ClientId(uint channel)
        {
            Value = channel;
        }

        public uint Value { get; }
    }
}