using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaMasks
{
    public class SnapshotDeltaEntityMasksHistory
    {
        private readonly Queue<SnapshotDeltaEntityMasks> masksQueue = new();
        
        public SnapshotDeltaEntityMasksUnion Fetch(TickIdRange range)
        {
            return new (range, new ());
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

        public void Enqueue(SnapshotDeltaEntityMasks entityMasksForTick)
        {
            masksQueue.Enqueue(entityMasksForTick);
        }
    }
}