/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.SnapshotProtocol;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class CompleteStatePack
    {
        public static ReadOnlySpan<byte> Pack(IEntityContainer world)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
            writer.WriteUInt8(0);
            EntityCountWriter.WriteEntityCount(world.EntityCount, writer);
            foreach (var entity in world.AllEntities)
            {
                PackCreatedEntity.Write(writer, entity.Id, entity.ArchetypeId, entity);
            }

            return writer.Octets;
        }
    }
}