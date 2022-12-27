/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public class AllEntitiesChangesThisTick
    {
        public readonly Dictionary<uint, EntityChangesForOneEntity> EntitiesComponentChanges = new();
        public TickId TickId;
    }
}