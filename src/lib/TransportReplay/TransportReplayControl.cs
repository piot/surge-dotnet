/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.TransportReplay
{
    public class TransportReplayControl : ITransportReplayControl
    {
        const int replayMemoryOctetSize = 600 * 1024;
        const int replayMemoryOctetThreshold = 28 * 1024;
        readonly SemanticVersion applicationVersion;
        readonly ILog log;
        readonly IMonotonicTimeMs timeProvider;
        readonly OctetWriter writer = new(replayMemoryOctetSize);
        IDisposableOctetWriter? disposableOctetWriter;
        TransportPlayback? playback;
        TransportRecorder? recorder;

        IOctetReaderWithSeekAndSkip? seekableOctetReader;

        public TransportReplayControl(SemanticVersion applicationVersion, IMonotonicTimeMs timeProvider, ILog log)
        {
            this.timeProvider = timeProvider;
            this.log = log;
            this.applicationVersion = applicationVersion;
        }

        public ITransportReceive StartRecordingToMemory(ITransportReceive transportToWrap,
            IOctetSerializableWrite state, TickId nowTickId)
        {
            if (recorder is not null)
            {
                throw new("is already recording");
            }

            recorder = new(transportToWrap, state, applicationVersion, timeProvider, nowTickId,
                writer);

            return recorder;
        }

        public ITransportReceive StartRecordingToFile(ITransportReceive transportToWrap, IOctetSerializableWrite state,
            TickId nowTickId, string filename)
        {
            log.Info("Start recording transport to {Filename}", filename);
            disposableOctetWriter = FileStreamCreator.Create(filename);
            if (transportToWrap is null)
            {
                throw new ArgumentNullException(nameof(transportToWrap));
            }

            recorder = new(transportToWrap, state, applicationVersion, timeProvider, nowTickId,
                disposableOctetWriter);

            return recorder;
        }

        public (ITransportReceive, TimeMs) StartPlaybackFromFile(IOctetSerializableRead state, string filename)
        {
            log.Info("Start playback transport from {Filename}", filename);
            seekableOctetReader?.Dispose();

            seekableOctetReader = FileStreamCreator.OpenWithSeek(filename);

            playback = new(state, applicationVersion, seekableOctetReader, timeProvider);


            return (playback, playback.InitialTimeMs);
        }

        public void Update(TickId tickId)
        {
            if (recorder != null)
            {
                recorder.TickId = tickId;
            }
        }

        public void StopRecording()
        {
            if (recorder is null)
            {
                log.Notice("You tried to stop recording, but we are not recording");
                return;
            }

            log.Info("Stopping transport recording");

            recorder.Close();
            recorder = null;

            disposableOctetWriter?.Dispose();
            disposableOctetWriter = null;
        }
    }
}