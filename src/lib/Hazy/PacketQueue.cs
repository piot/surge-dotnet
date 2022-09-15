/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.MonotonicTime;
using Piot.Transport;

namespace Piot.Hazy
{
    public interface IPacketQueuePop
    {
        bool Dequeue(TimeMs atOrBeforeMs, out Packet packet);
    }

    public sealed class PacketQueue : IPacketQueuePop
    {
        private readonly LinkedList<Packet> queue = new();

        public int Count => queue.Count;

        /// <summary>
        ///     Dequeues the first packet that are <paramref name="atOrBeforeMs" /> the time.
        /// </summary>
        /// <param name="atOrBeforeMs"></param>
        /// <param name="packet"></param>
        /// <returns>true if a packet exists that have a timestamp equal or less than <paramref name="atOrBeforeMs" /></returns>
        public bool Dequeue(TimeMs atOrBeforeMs, out Packet packet)
        {
            foreach (var queuedPacket in queue)
            {
                if (!queuedPacket.monotonicTimeMs.IsBeforeOrAt(atOrBeforeMs))
                {
                    continue;
                }

                packet = queuedPacket;

                queue.Remove(queuedPacket);

                return true;
            }

            packet = new();
            return false;
        }

        /// <summary>
        ///     Add packet to packet queue
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

        /// <summary>
        ///     Finds a the packet with the lowest timestamp that is scheduled to be sent to the <paramref name="endpointId" />.
        ///     Mostly used for reordering logic in Internet Simulation.
        /// </summary>
        /// <param name="endpointId"></param>
        /// <param name="foundPacket"></param>
        /// <returns></returns>
        public bool FindFirstPacketForEndpoint(RemoteEndpointId endpointId, out Packet foundPacket)
        {
            foreach (var queuedPacket in queue)
            {
                if (queuedPacket.endPoint.Value == endpointId.Value)
                {
                    foundPacket = queuedPacket;
                    return true;
                }
            }

            foundPacket = new();
            return false;
        }
    }
}