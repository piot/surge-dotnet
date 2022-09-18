/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class TransportRecorder : ITransportSend
    {
        private readonly OctetWriter cachedBuffer = new(1800);
        private readonly IMonotonicTimeMs timeProvider;
        private readonly ReplayWriter writer;

        public TransportRecorder(ClientDatagramReceiver receiver, SemanticVersion applicationSemanticVersion,
            IMonotonicTimeMs timeProvider, TickId tickId, IOctetWriter target)
        {
            TickId = tickId;
            this.timeProvider = timeProvider;
            var stateWriter = new OctetWriter(32 * 1024);
            receiver.Serialize(stateWriter);
            var complete = new CompleteState(timeProvider.TimeInMs, tickId, stateWriter.Octets);

            writer = new(complete, new(applicationSemanticVersion, SurgeConstants.SnapshotSerializationVersion),
                target);
        }

        public TickId TickId { get; set; }

        public void SendToEndpoint(RemoteEndpointId remoteEndpointId, ReadOnlySpan<byte> datagram)
        {
            cachedBuffer.Reset();
            cachedBuffer.WriteUInt16(remoteEndpointId.Value);
            cachedBuffer.WriteUInt16((ushort)datagram.Length);
            cachedBuffer.WriteOctets(datagram);

            var delta = new DeltaState(timeProvider.TimeInMs, TickIdRange.FromTickId(TickId), cachedBuffer.Octets);
            writer.AddDeltaState(delta);
        }
    }
}