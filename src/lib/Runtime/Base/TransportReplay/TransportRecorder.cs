/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Replay.Serialization;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class TransportRecorder : ITransportReceive
    {
        readonly OctetWriter cachedBuffer = new(Transport.Constants.MaxDatagramOctetSize + 4);
        readonly IMonotonicTimeMs timeProvider;
        readonly ITransportReceive wrappedTransport;
        readonly ReplayWriter writer;

        public TransportRecorder(ITransportReceive wrappedTransport, IOctetSerializableWrite state,
            SemanticVersion applicationSemanticVersion,
            IMonotonicTimeMs timeProvider, TickId tickId, IOctetWriter target)
        {
            this.wrappedTransport = wrappedTransport ?? throw new ArgumentNullException(nameof(wrappedTransport));
            TickId = tickId;
            this.timeProvider = timeProvider;
            var stateWriter = new OctetWriter(32 * 1024);
            state.Serialize(stateWriter);
            var complete = new CompleteState(timeProvider.TimeInMs, tickId, stateWriter.Octets);
            const int framesBetweenCompleteState = 0;
            writer = new(complete, new(applicationSemanticVersion, SurgeConstants.SnapshotSerializationVersion),
                Constants.ReplayInfo,
                target, framesBetweenCompleteState);
        }

        public TickId TickId { get; set; }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            var octets = wrappedTransport.Receive(out endpointId);
            if (octets.IsEmpty)
            {
                return octets;
            }

            Write(endpointId, octets);
            return octets;
        }

        public void Write(EndpointId endpointId, ReadOnlySpan<byte> datagram)
        {
            cachedBuffer.Reset();
            cachedBuffer.WriteUInt16(endpointId.Value);
            cachedBuffer.WriteUInt16((ushort)datagram.Length);
            cachedBuffer.WriteOctets(datagram);

            var delta = new DeltaState(timeProvider.TimeInMs, TickIdRange.FromTickId(TickId), cachedBuffer.Octets);
            writer.AddDeltaState(delta);
        }

        public void Close()
        {
            writer.Close();
        }
    }
}