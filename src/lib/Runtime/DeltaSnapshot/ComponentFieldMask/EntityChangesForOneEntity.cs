/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public class EntityChangesForOneEntity
    {
        public readonly Dictionary<ComponentTypeId, ComponentChangedFieldMask> componentChanges = new();
        public EntityId entityId;

        public EntityChangesForOneEntity(EntityId entityId)
        {
            this.entityId = entityId;
        }

        public void Add(ComponentTypeId componentTypeId, ComponentChangedFieldMask fieldMasks)
        {
            componentChanges.Add(componentTypeId, fieldMasks);
        }
    }
}