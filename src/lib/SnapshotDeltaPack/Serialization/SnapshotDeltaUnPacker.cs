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
        ///     Helper method for calling <see cref="SnapshotDeltaReader.Read" />.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static (IEntity[] deletedEntities, IEntity[]createdEntities,
            SnapshotDeltaReaderInfoEntity[] updatedEntities) UnPack(Memory<byte> pack, IEntityContainerWithCreation entityContainer)
        {
            var reader = new OctetReader(pack);
            var (deletedEntities, createdEntities, updatedEntities) = SnapshotDeltaReader.Read(reader, entityContainer);

            return (deletedEntities, createdEntities, updatedEntities);
        }
    }
}