/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.ChangeMask;
using Piot.Surge.ChangeMaskSerialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class PackUpdatedEntity
    {
        public static void Write(IOctetWriter writer, EntityId entityId, ChangedFieldsMask changeMask,
            IEntitySerializer entitySerializer)
        {
            EntityIdWriter.Write(writer, entityId);
            ChangedFieldsMaskWriter.WriteChangedFieldsMask(writer, changeMask);
            entitySerializer.Serialize(changeMask.mask, writer);
        }
    }
}