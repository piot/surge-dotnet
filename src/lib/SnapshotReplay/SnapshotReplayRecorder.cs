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
        private const int replayMemoryOctetSize = 32 * 1024;
        private const int replayMemoryOctetThreshold = 28 * 1024;
        private readonly SemanticVersion applicationVersion;
        private readonly IEntityContainer entityContainer;
        private readonly IEventProcessor eventProcessor;
        private readonly ILog log;
        private readonly OctetWriter writer = new(replayMemoryOctetSize);
        private IDisposableOctetWriter? disposableOctetWriter;
        private ReplayPlayback? playback;
        private readonly IEntityContainerWithGhostCreator playbackWorld;
        private ReplayRecorder? recorder;
        private IOctetReaderWithSeekAndSkip? seekableOctetReader;

        public SnapshotReplayRecorder(SemanticVersion applicationVersion, IEntityContainer recordWorld,
            IEntityContainerWithGhostCreator playbackWorld, IEventProcessor eventProcessor, ILog log)
        {
            this.log = log;
            this.applicationVersion = applicationVersion;
            entityContainer = recordWorld ?? throw new ArgumentNullException(nameof(recordWorld));
            this.playbackWorld = playbackWorld;
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

            recorder = new(entityContainer, timeNowMs, nowTickId, applicationVersion, writer, log);
        }

        public void StartRecordingToFile(TimeMs timeNowMs, TickId nowTickId, string filename)
        {
            disposableOctetWriter = FileStreamCreator.Create(filename);
            recorder = new(entityContainer, timeNowMs, nowTickId, applicationVersion, disposableOctetWriter, log);
        }

        public void StartPlaybackFromFile(TimeMs now, string filename)
        {
            seekableOctetReader?.Dispose();

            playbackWorld.Reset();
            seekableOctetReader = FileStreamCreator.OpenWithSeek(filename);
            playback = new(playbackWorld, eventProcessor, now, applicationVersion, seekableOctetReader,
                log);
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