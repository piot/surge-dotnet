/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.FieldMask;
using Piot.Surge.FieldMask.Serialization;
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
        public static void Write(IOctetWriter writer, EntityId entityId, FieldMask.ChangedFieldsMask changeMask,
            IEntitySerializer entitySerializer)
        {
            EntityIdWriter.Write(writer, entityId);
            ChangedFieldsMaskWriter.WriteChangedFieldsMask(writer, changeMask);
            entitySerializer.Serialize(changeMask.mask, writer);
        }
    }
}