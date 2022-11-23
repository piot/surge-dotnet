/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.MonotonicTime;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public delegate void SnapshotPlaybackNotify(TimeMs now, TickId tickIdNow, DeltaSnapshotPack deltaSnapshotPack);
}