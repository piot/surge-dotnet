/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;
using Piot.Surge.Tick;

namespace Piot.Surge.SnapshotReplay
{
    public interface IReplayControl
    {
        public TickId MinTickId { get; }
        public TickId MaxTickId { get; }

        public TickId TickId { get; }
        public void StartRecordingToMemory(TimeMs timeNowMs, TickId tickIdNow);
        public void StartRecordingToFile(TimeMs timeNowMs, TickId nowTickId, string filename);
        public void StopRecording();

        public void LoadPlaybackFromFile(TimeMs now, string filename);

        public TickId Seek(TickId tickId);

        public void Play();

        public void Stop();
        public void Update(TimeMs timeNow);
    }
}