/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    internal static class SnapshotDeltaWriter
    {
        private static void WriteEntityCount(IOctetWriter writer, int entityCount)
        {
            writer.WriteUInt16((ushort)entityCount);
        }

        /// <summary>
        ///     Serialize deleted, created and updated entities to the writer stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="deltaMemory"></param>
        internal static void Write(SnapshotDeltaMemory deltaMemory, IOctetWriter writer)
        {
#if DEBUG
            writer.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaSync);
#endif
            if (deltaMemory.deletedCount == 0 && deltaMemory.createdCount == 0 && deltaMemory.updatedCount == 0)
            {
                throw new Exception("suspicious, nothing has changed in this delta");
            }

            WriteEntityCount(writer, (int)deltaMemory.deletedCount);
            writer.WriteOctets(deltaMemory.deletedMemory);

#if DEBUG
            writer.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaCreatedSync);
#endif
            WriteEntityCount(writer, (int)deltaMemory.createdCount);
            writer.WriteOctets(deltaMemory.createdMemory);

#if DEBUG
            writer.WriteUInt8(SnapshotSerialization.Constants.SnapshotDeltaUpdatedSync);
#endif
            WriteEntityCount(writer, (int)deltaMemory.updatedCount);
            writer.WriteOctets(deltaMemory.updatedMemory);
        }
    }
}