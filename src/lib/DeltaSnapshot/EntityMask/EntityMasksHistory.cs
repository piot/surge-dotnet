/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.EntityMask
{
    public sealed class EntityMasksHistory
    {
        readonly Queue<EntityMasks> masksQueue = new();

        public EntityMasksUnion Fetch(TickIdRange range)
        {
            return EntityMasksMerger.Merge(masksQueue.Where(mask => range.Contains(mask.TickId)).ToArray());
        }

        public void DiscardUpTo(TickId tickId)
        {
            while (masksQueue.Count > 0)
            {
                if (masksQueue.Peek().TickId.tickId >= tickId.tickId)
                {
                    break;
                }

                masksQueue.Dequeue();
            }
        }

        public void Enqueue(EntityMasks entityMasksForTick)
        {
            masksQueue.Enqueue(entityMasksForTick);
        }
    }
}