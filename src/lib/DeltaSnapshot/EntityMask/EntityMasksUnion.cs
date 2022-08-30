/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.DeltaSnapshot.EntityMask
{
    /// <summary>
    ///     A union of entity mask changes that happened in the <see cref="TickIdRange" /> of delta snapshots.
    /// </summary>
    public class EntityMasksUnion
    {
        public EntityMasksUnion(TickIdRange tickIdRange, Dictionary<ushort, ulong> entityToMask)
        {
            TickIdRange = tickIdRange;
            EntityMasks = entityToMask;
        }

        public Dictionary<ushort, ulong> EntityMasks { get; }

        public TickIdRange TickIdRange { get; }
    }
}