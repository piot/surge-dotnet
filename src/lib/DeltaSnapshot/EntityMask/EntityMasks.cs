/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.DeltaSnapshot.EntityMask
{
    /// <summary>
    ///     <see cref="FieldMask" /> for all the entities that changed this tick.
    /// </summary>
    public class EntityMasks
    {
        public EntityMasks(TickId tickId, Dictionary<ushort, ulong> toMask)
        {
            TickId = tickId;
            Masks = toMask;
        }

        public EntityMasks(EntityMasksMutable mutable)
        {
            TickId = mutable.TickIdRange.Last;
            Masks = mutable.Masks;
        }

        public Dictionary<ushort, ulong> Masks { get; }

        public TickId TickId { get; }

        public ulong FetchEntity(EntityId entityId)
        {
            return Masks[entityId.Value];
        }
    }
}