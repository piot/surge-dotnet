/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Pulse.Client;
using Piot.Surge.Tick;

namespace Tests;

public class MockPlaybackNotify : ISnapshotPlaybackNotify
{
    public void SnapshotPlaybackNotify(TimeMs now, TickId tickIdNow, DeltaSnapshotPack deltaSnapshotPack)
    {
    }
}