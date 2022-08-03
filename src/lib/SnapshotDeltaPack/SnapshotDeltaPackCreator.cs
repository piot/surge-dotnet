/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaPackCreator
    {
        /// <summary>
        ///     Creates a snapshot delta from the current state of the World. Almost always being produced on the simulator
        /// </summary>
        /// <param name="idRange"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static SnapshotDeltaPack Create(SnapshotId snapshotId, World world, SnapshotDelta.SnapshotDelta delta)
        {
            var payload = SnapshotDeltaPackPayloadCreator.CreatePack(world, delta);

            return new SnapshotDeltaPack(snapshotId, payload);
        }
    }
}