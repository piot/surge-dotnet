/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;

namespace Piot.Surge.SnapshotDeltaInternal
{
    public static class SnapshotDeltaInternalMerger
    {
        /// <summary>
        ///     Merges (creates a union of) all the masks in the SnapshotDeltaInternals passed in.
        /// </summary>
        /// <param name="deltas">Array of SnapshotDeltaInternal</param>
        /// <returns>A complete SnapshotDelta</returns>
        public static SnapshotDelta.SnapshotDelta Merge(SnapshotDeltaInternal[] deltas)
        {
            var baseDelta = new SnapshotDeltaInternal();
            foreach (var delta in deltas)
            foreach (var entityInfo in delta.entities)
            {
                baseDelta.entities.TryGetValue(entityInfo.Key, out var foundInfo);
                if (foundInfo is not null)
                    foundInfo.changeMask = FullChangeMask.MergeBits(foundInfo.changeMask, entityInfo.Value.changeMask);
                else
                    baseDelta.entities[entityInfo.Key] =
                        new SnapshotDeltaInternalInfoEntity(entityInfo.Value.entityId, entityInfo.Value.changeMask);
            }

            return FromSnapshotDeltaInternal.Convert(baseDelta);
        }
    }
}