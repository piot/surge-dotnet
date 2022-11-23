/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Ecs2;
using Piot.Clog;
using Piot.MonotonicTime;
using Piot.SerializableVersion;
using Piot.Surge.Core;
using Piot.Surge.SnapshotReplay;
using Piot.Surge.TransportReplay;

namespace Surge.Game
{
    public struct GameToolsInfo
    {
        public IEventReceiver eventProcessor;
        public IMonotonicTimeMs timeProvider;
        public IDataReceiver clientWorld;
        public IDataSender readWorld;
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
                new(gameVersion, toolsInfo.readWorld, toolsInfo.clientWorld,
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