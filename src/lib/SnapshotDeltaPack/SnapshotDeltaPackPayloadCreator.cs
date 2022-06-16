/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Surge.SnapshotDeltaPack;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    internal static class SnapshotDeltaPackPayloadCreator
    {
        internal static Memory<byte> CreatePack(IEntityContainer world, SnapshotDelta.SnapshotDelta snapshotDeltaAfter)
        {
            var createdEntities = snapshotDeltaAfter.createdIds.Select(world.FetchEntity).ToArray();
            var updatedEntities = snapshotDeltaAfter.updatedEntities.Select(x =>
                (IUpdatedEntity)new UpdatedEntity(x.entityId, x.changeMask, world.FetchEntity(x.entityId))).ToArray();

            var snapshotDeltaPackPayload =
                SnapshotDeltaPacker.Pack(snapshotDeltaAfter.deletedIds, createdEntities, updatedEntities);

            var updatedEntityIds = snapshotDeltaAfter.updatedEntities
                .Select(updatedEntityInfo => updatedEntityInfo.entityId).ToArray();

            OverWriter.Overwrite(world, updatedEntityIds);

            return snapshotDeltaPackPayload;
        }
    }
}