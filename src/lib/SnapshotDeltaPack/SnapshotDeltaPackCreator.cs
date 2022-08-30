/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaPackCreator
    {
        /// <summary>
        ///     Creates a snapshot delta from the current state of the World. Almost always being produced on the host (simulator)
        /// </summary>
        /// <param name="idRange"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static DeltaSnapshotPack Create(IEntityContainer world, DeltaSnapshot.DeltaSnapshotEntityIds delta)
        {
            return DeltaSnapshotToPack.ToDeltaSnapshotPack(world, delta);
        }
    }
}