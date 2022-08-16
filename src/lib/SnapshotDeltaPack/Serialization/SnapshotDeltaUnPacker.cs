/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaUnPacker
    {
        /// <summary>
        ///     Helper method for calling SnapshotDeltaReader.Read.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static (IEntity[] deletedEntities, IEntity[]createdEntities,
            SnapshotDeltaReaderInfoEntity[] updatedEntities) UnPack(Memory<byte> pack, IEntityContainer creator)
        {
            var reader = new OctetReader(pack);
            var (deletedEntities, createdEntities, updatedEntities) = SnapshotDeltaReader.Read(reader, creator);

            return (deletedEntities, createdEntities, updatedEntities);
        }
    }
}