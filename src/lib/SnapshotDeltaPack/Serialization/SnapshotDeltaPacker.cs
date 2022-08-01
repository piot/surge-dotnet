/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Surge.SnapshotDeltaPack;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaPacker
    {
        public static Memory<byte> Pack(EntityId[] deletedEntities, IEntity[] createdEntities,
            IUpdatedEntity[] updatedEntities)
        {
            var writer = new OctetWriter(32 * 1024);
            SnapshotDeltaWriter.Write(writer, deletedEntities, createdEntities, updatedEntities);

            return writer.Octets;
        }
    }
}