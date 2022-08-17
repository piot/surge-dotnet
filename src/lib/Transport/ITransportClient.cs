using System;

namespace Piot.Surge.Transport
{
    public interface ITransportClient
    {
        public void SendToHost(Memory<byte> payload);
        public Memory<byte> ReceiveFromHost();
    }
}