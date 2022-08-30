using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.EntityMask
{
    public class EntityMasksHistory
    {
        private readonly Queue<EntityMasks> masksQueue = new();

        public EntityMasksUnion Fetch(TickIdRange range)
        {
            return new(range, new());
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