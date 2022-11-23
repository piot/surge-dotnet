/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System.Collections.Generic;
using Piot.Surge.FieldMask;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public struct ComponentChangesForOneEntityImmutable
    {
        public EntityId entityId;
        public readonly Dictionary<ushort, ulong> componentChangesMasks;
        ushort[] deletedComponentTypeIds;

        public ComponentChangesForOneEntityImmutable(EntityId entityId, Dictionary<ushort, ulong> componentChanges)
        {
            componentChangesMasks = componentChanges;
            var deleted = new List<ushort>();
            foreach (var componentChangePair in componentChanges)
            {
                if ((componentChangePair.Value & ChangedFieldsMask.DeletedMaskBit) == ChangedFieldsMask.DeletedMaskBit)
                {
                    deleted.Add(componentChangePair.Key);
                }
            }

            deletedComponentTypeIds = deleted.ToArray();

            this.entityId = entityId;
        }
    }
}