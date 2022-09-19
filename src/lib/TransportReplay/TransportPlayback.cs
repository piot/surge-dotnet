/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Replay.Serialization;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class TransportPlayback : ITransportReceive
    {
        private readonly ReplayReader replayPlayback;
        private readonly IMonotonicTimeMs timeProvider;
        private bool isEndOfStream;
        private DeltaState nextDeltaState;

        public TransportPlayback(IOctetSerializableRead state, SemanticVersion applicationSemanticVersion,
            IOctetReaderWithSeekAndSkip readerWithSeekAndSkip, IMonotonicTimeMs timeProvider)
        {
            this.timeProvider = timeProvider;
            replayPlayback = new(applicationSemanticVersion, readerWithSeekAndSkip);
            var completeState = replayPlayback.Seek(new(0));

            var reader = new OctetReader(completeState.Payload);
            state.Deserialize(reader);

            var deltaState = replayPlayback.ReadDeltaState();

            nextDeltaState = deltaState ?? throw new Exception("too short");
        }

        public ReadOnlySpan<byte> Receive(out EndpointId endpointId)
        {
            if (timeProvider.TimeInMs.ms < nextDeltaState.TimeProcessedMs.ms || isEndOfStream)
            {
                endpointId = EndpointId.NoEndpoint;
                return ReadOnlySpan<byte>.Empty;
            }

            var reader = new OctetReader(nextDeltaState.Payload);

            endpointId = new EndpointId(reader.ReadUInt16());
            var octetLength = reader.ReadUInt16();

            var octetsToReturn = reader.ReadOctets(octetLength);
            var next = replayPlayback.ReadDeltaState();
            if (next is null)
            {
                isEndOfStream = true;
            }
            else
            {
                nextDeltaState = next;
            }

            return octetsToReturn;
        }
    }
}