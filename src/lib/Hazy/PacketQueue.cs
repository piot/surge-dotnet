/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.MonotonicTime;

namespace Piot.Hazy
{
    public class PacketQueue
    {
        private readonly LinkedList<Packet> queue = new();

        /// <summary>
        /// Add packet to packet queue
        /// </summary>
        /// <param name="insertAtMs"></param>
        /// <param name="packet"></param>
        public void AddPacket(Packet packet)
        {
            var element = queue.Last;
            while (element != null)
            {
                if (element.Value.monotonicTimeMs.ms < packet.monotonicTimeMs.ms)
                {
                    queue.AddAfter(element, packet);
                    return;
                }

                element = element.Previous;
            }

            queue.AddFirst(packet);
        }

        public bool Dequeue(Milliseconds atOrBeforeMs, out Packet packet)
        {
            foreach (var queuedPacket in queue)
            {
                if (queuedPacket.monotonicTimeMs.IsBeforeOrAt(atOrBeforeMs))
                {
                    packet = queuedPacket;

                    queue.Remove(queuedPacket);

                    return true;
                }
            }

            packet = new();
            return false;
        }

        public int Count => queue.Count;
    }
}