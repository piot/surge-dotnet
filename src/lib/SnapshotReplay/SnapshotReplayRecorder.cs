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
        private readonly IEntityContainerWithGhostCreator entityContainer;
        private readonly IEventProcessor eventProcessor;
        private readonly ILog log;
        private readonly OctetWriter writer = new(replayMemoryOctetSize);
        private IDisposableOctetWriter? disposableOctetWriter;
        private ReplayPlayback? playback;
        private ReplayRecorder? recorder;
        private IOctetReaderWithSeekAndSkip? seekableOctetReader;

        public SnapshotReplayRecorder(SemanticVersion applicationVersion, IEntityContainerWithGhostCreator world,
            IEventProcessor eventProcessor, ILog log)
        {
            this.log = log;
            this.applicationVersion = applicationVersion;
            entityContainer = world ?? throw new ArgumentNullException(nameof(world));
            this.eventProcessor = eventProcessor;
        }

        public void StartRecordingToMemory(TickId nowTickId)
        {
            if (recorder is not null)
            {
                return;
            }

            recorder = new(entityContainer, nowTickId, applicationVersion, writer, log);
        }

        public void StartRecordingToFile(TickId nowTickId, string filename)
        {
            disposableOctetWriter = FileStreamCreator.Create(filename);
            recorder = new(entityContainer, nowTickId, applicationVersion, disposableOctetWriter, log);
        }

        public void StartPlaybackFromFile(TimeMs now, string filename)
        {
            seekableOctetReader?.Dispose();

            entityContainer.Reset();
            seekableOctetReader = FileStreamCreator.OpenWithSeek(filename);
            playback = new(entityContainer, eventProcessor, now, applicationVersion, seekableOctetReader,
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

        public void SnapshotPlaybackNotify(TimeMs now, TickId tickIdNow, DeltaSnapshotPack deltaSnapshotPack)
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

            recorder.AddPack(deltaSnapshotPack, tickIdNow);
        }
    }
}