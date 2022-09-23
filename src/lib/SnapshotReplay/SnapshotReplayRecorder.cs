/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Event;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Replay;
using Piot.Surge.Tick;

namespace Piot.Surge.SnapshotReplay
{
    public class SnapshotReplayRecorder : IReplayControl, ISnapshotPlaybackNotify
    {
        const int replayMemoryOctetSize = 32 * 1024;
        const int replayMemoryOctetThreshold = 28 * 1024;
        readonly SemanticVersion applicationVersion;
        readonly IEntityContainer entityContainer;
        readonly IEventProcessor eventProcessor;
        readonly ILog log;
        readonly IEntityContainerWithGhostCreator playbackWorld;
        readonly OctetWriter writer = new(replayMemoryOctetSize);
        IDisposableOctetWriter? disposableOctetWriter;
        readonly INotifyEntityCreation notifyEntityCreation;
        ReplayPlayback? playback;
        ReplayRecorder? recorder;
        IOctetReaderWithSeekAndSkip? seekableOctetReader;

        public SnapshotReplayRecorder(SemanticVersion applicationVersion, IEntityContainer recordWorld,
            IEntityContainerWithGhostCreator playbackWorld, INotifyEntityCreation notifyEntityCreation,
            IEventProcessor eventProcessor, ILog log)
        {
            this.log = log;
            this.applicationVersion = applicationVersion;
            entityContainer = recordWorld ?? throw new ArgumentNullException(nameof(recordWorld));
            this.playbackWorld = playbackWorld;
            this.notifyEntityCreation = notifyEntityCreation;
            this.eventProcessor = eventProcessor;
        }

        public FixedDeltaTimeMs DeltaTime
        {
            set
            {
                if (playback is not null)
                {
                    playback.DeltaTime = value;
                }
            }
        }

        public void StartRecordingToMemory(TimeMs timeNowMs, TickId nowTickId)
        {
            if (recorder is not null)
            {
                return;
            }

            recorder = new(entityContainer, timeNowMs, nowTickId, applicationVersion, Constants.ReplayInfo, writer,
                log);
        }

        public void StartRecordingToFile(TimeMs timeNowMs, TickId nowTickId, string filename)
        {
            disposableOctetWriter = FileStreamCreator.Create(filename);
            recorder = new(entityContainer, timeNowMs, nowTickId, applicationVersion, Constants.ReplayInfo,
                disposableOctetWriter, log);
        }

        public void LoadPlaybackFromFile(TimeMs now, string filename)
        {
            if (seekableOctetReader is not null)
            {
                seekableOctetReader?.Dispose();
                playback = null;
            }

            playbackWorld.Reset();
            seekableOctetReader = FileStreamCreator.OpenWithSeek(filename);
            playback = new(playbackWorld, notifyEntityCreation, eventProcessor, now, applicationVersion,
                Constants.ReplayInfo,
                seekableOctetReader,
                log);
            playback.DeltaTime = new(0);
        }

        public TickId Seek(TickId tickId)
        {
            return playback?.Seek(tickId) ?? tickId;
        }

        public void Play()
        {
            if (playback is null)
            {
                return;
            }

            playback.DeltaTime = new(20);
        }

        public TickId MinTickId => playback?.MinTickId ?? new(0);
        public TickId MaxTickId => playback?.MaxTickId ?? new(0);
        public TickId TickId => playback?.TickId ?? new(0);

        public void Stop()
        {
            if (seekableOctetReader is null)
            {
                return;
            }

            seekableOctetReader.Dispose();
            playback = null;
        }

        public void Update(TimeMs timeNow)
        {
            playback?.Update(timeNow);
        }

        public void StopRecording()
        {
            if (recorder is null)
            {
                log.Notice("You tried to stop recording, but we are not recording");
                return;
            }

            recorder.Close();
            recorder = null;

            disposableOctetWriter?.Dispose();
            disposableOctetWriter = null;
        }


        public void SnapshotPlaybackNotify(TimeMs timeNowMs, TickId tickIdNow, DeltaSnapshotPack deltaSnapshotPack)
        {
            if (recorder is null)
            {
                return;
            }

            if (writer.Position >= replayMemoryOctetThreshold)
            {
                log.Notice("replay buffer full, closing it now");
                StopRecording();
                return;
            }

            recorder.AddPack(deltaSnapshotPack, timeNowMs, tickIdNow);
        }
    }
}