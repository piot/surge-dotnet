/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge;
using Piot.Surge.Event;
using Piot.Surge.SnapshotReplay;
using Piot.Surge.TransportReplay;

namespace Surge.Game
{
    public struct GameToolsInfo
    {
        public IEntityContainerWithGhostCreator clientWorld;
        public IEntityContainerWithGhostCreator playbackWorld;
        public INotifyEntityCreation worldSync;
        public IEventProcessor eventProcessor;
        public IMonotonicTimeMs timeProvider;
    }

    public class GameTools
    {
        readonly TransportReplayControl transportReplayRecorder;

        GameToolsInfo toolsInfo;

        public GameTools(SemanticVersion gameVersion, GameToolsInfo toolsInfo, ILog log)
        {
            this.toolsInfo = toolsInfo;
            transportReplayRecorder = new(gameVersion, toolsInfo.timeProvider, log.SubLog("TransportReplay"));

            RawSnapshotReplayRecorder =
                new(gameVersion, toolsInfo.clientWorld, toolsInfo.playbackWorld, toolsInfo.worldSync,
                    toolsInfo.eventProcessor,
                    log.SubLog("SnapshotRecorder"))
                {
                    DeltaTime = new(16 * 10)
                };
        }

        public SnapshotReplayRecorder RawSnapshotReplayRecorder { get; }

        public IReplayControl ReplayControl => RawSnapshotReplayRecorder;
        public ITransportReplayControl TransportReplayControl => transportReplayRecorder;

        public void Update(TimeMs now)
        {
            RawSnapshotReplayRecorder.Update(now);
            //transportReplayRecorder.Update(now);
        }
    }
}