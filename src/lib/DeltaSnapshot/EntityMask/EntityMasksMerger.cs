/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Surge.Snapshot;

namespace Piot.Surge.DeltaSnapshot.EntityMask
{
    public static class EntityMasksMerger
    {
        /// <summary>
        ///     Merges an array of <paramref name="containers" /> and returns a union of those encountered masks.
        /// </summary>
        /// <param name="containers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="Exception"></exception>
        public static EntityMasksUnion Merge(EntityMasks[] containers)
        {
            if (containers.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(containers),
                    "must provide at least one container to merge");
            }

            var tickIdRange = new TickIdRange(containers[0].TickId, containers.Last().TickId);
            var mutableContainer = new EntityMasksMutable(tickIdRange);

#if DEBUG
            var nextExpectedTickValue = containers[0].TickId.tickId;
#endif
            foreach (var container in containers)
            {
#if DEBUG
                if (container.TickId.tickId != nextExpectedTickValue)
                {
                    throw new Exception(
                        $"containers are in wrong order. expected {nextExpectedTickValue} but got {container.TickId}");
                }
#endif
                foreach (var entityMaskPair in container.Masks)
                {
                    mutableContainer.Merge(entityMaskPair.Key, entityMaskPair.Value);
                }
#if DEBUG
                nextExpectedTickValue++;
#endif
            }


            return new(tickIdRange, mutableContainer.Masks);
        }
    }
}