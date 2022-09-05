/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateBitWriter
    {
        public static ReadOnlySpan<byte> CaptureCompleteSnapshotPack(IEntityContainer world)
        {
            var writer = new BitWriter(SnapshotProtocol.Constants.MaxDatagramOctetSize);
#if DEBUG
            BitMarker.WriteMarker(writer, Constants.CompleteSnapshotStartMarker);
#endif
            EntityCountWriter.WriteEntityCount(world.EntityCount, writer);
            foreach (var entityToSerialize in world.AllEntities)
            {
                EntityIdWriter.Write(writer, entityToSerialize.Id);
                writer.WriteBits(entityToSerialize.ArchetypeId.id, 10);
                entityToSerialize.SerializeAll(writer);
            }

            return writer.Close(out _);
        }
    }
}