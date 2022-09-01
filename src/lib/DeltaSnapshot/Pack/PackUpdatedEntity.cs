/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.FieldMask;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class PackUpdatedEntity
    {
        /// <summary>
        ///     Write an updated Entity. EntityId, FieldsMask, and the changed fields for the Entity.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="entityId"></param>
        /// <param name="changeMask"></param>
        /// <param name="entitySerializer"></param>
        public static void Write(IOctetWriter writer, EntityId entityId, ChangedFieldsMask changeMask,
            IEntitySerializer entitySerializer)
        {
            EntityIdWriter.Write(writer, entityId);
            entitySerializer.Serialize(changeMask.mask, writer);
        }

        /// <summary>
        ///     Write an updated Entity. EntityId, FieldsMask, and the changed fields for the Entity.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="entityId"></param>
        /// <param name="changeMask"></param>
        /// <param name="entitySerializer"></param>
        public static void Write(IBitWriter writer, EntityId entityId, ChangedFieldsMask changeMask,
            IEntityBitSerializer entitySerializer)
        {
            EntityIdWriter.Write(writer, entityId);
            entitySerializer.Serialize(changeMask.mask, writer);
        }
    }
}