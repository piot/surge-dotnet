/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    /*
    public static class SnapshotDeltaUnPacker
    {
        /// <summary>
        ///     Helper method for calling <see cref="SnapshotDeltaReader.Read" />.
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        public static (IEntity[] deletedEntities, IEntity[]createdEntities,
            SnapshotDeltaReaderInfoEntity[] updatedEntities) UnPack(ReadOnlySpan<byte> pack,
                IEntityContainerWithGhostCreator entityGhostContainer)
        {
            var reader = new OctetReader(pack.ToArray());
            var (deletedEntities, createdEntities, updatedEntities) =
                SnapshotDeltaReader.Read(reader, entityGhostContainer);

            return (deletedEntities, createdEntities, updatedEntities);
        }
    }
    */
}