/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public interface ITransportReplayControl
    {
        public ITransportReceive StartRecordingToMemory(ITransportReceive transportToWrap,
            IOctetSerializableWrite state, TickId nowTickId);

        public ITransportReceive StartRecordingToFile(ITransportReceive transportToWrap, IOctetSerializableWrite state,
            TickId nowTickId, string filename);
        public void StopRecording();

        (ITransportReceive, TimeMs) StartPlaybackFromFile(IOctetSerializableRead state, string filename);
        public void Update(TickId tickId);
    }
}