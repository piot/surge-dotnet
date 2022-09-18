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
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class TransportPlayback : ITransportReceive
    {
        private readonly DeltaState nextDeltaState;
        private readonly IMonotonicTimeMs timeProvider;
        private readonly ReplayReader replayPlayback;

        public TransportPlayback(ClientDatagramReceiver receiver, SemanticVersion applicationSemanticVersion,
            IOctetReaderWithSeekAndSkip readerWithSeekAndSkip, IMonotonicTimeMs timeProvider)
        {
            this.timeProvider = timeProvider;
            replayPlayback = new(applicationSemanticVersion, readerWithSeekAndSkip);
            var completeState = replayPlayback.Seek(new(0));

            var reader = new OctetReader(completeState.Payload);
            receiver.Deserialize(reader);

            var deltaState = replayPlayback.ReadDeltaState();

            nextDeltaState = deltaState ?? throw new Exception("too short");
        }

        public ReadOnlySpan<byte> Receive(out RemoteEndpointId remoteEndpointId)
        {
            if (timeProvider.TimeInMs.ms < nextDeltaState.TimeProcessedMs.ms)
            {
                remoteEndpointId = new(0);
                return ReadOnlySpan<byte>.Empty;
            }

            var reader = new OctetReader(nextDeltaState.Payload);

            remoteEndpointId = new RemoteEndpointId(reader.ReadUInt16());
            var octetLength = reader.ReadUInt16();

            return reader.ReadOctets(octetLength);
        }
    }
}