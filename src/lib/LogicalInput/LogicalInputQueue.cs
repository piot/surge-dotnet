/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Surge.Tick;

namespace Piot.Surge.LogicalInput
{
    /// <summary>
    ///     Queue of serialized game specific inputs.
    ///     Used by the predicting client to keep track of inputs that needs to be
    ///     re-simulated when a correction state is received.
    /// </summary>
    public class LogicalInputQueue
    {
        private readonly Queue<LogicalInput> queue = new();

        private TickId waitingForTickId;

        public LogicalInput[] Collection => queue.ToArray();

        public int Count => queue.Count;

        public TickId WaitingForTickId => waitingForTickId;

        public bool IsInitialized { get; private set; }

        public void AddLogicalInput(LogicalInput logicalInput)
        {
            if (!IsInitialized)
            {
                waitingForTickId = logicalInput.appliedAtTickId;
                IsInitialized = true;
            }

            if (logicalInput.appliedAtTickId.tickId > waitingForTickId.tickId)
            {
                // TickId can only go up, so it means that we have dropped inputs at previous ticks
                // We can only remove all inputs and add this one
                Reset();
            }

            queue.Enqueue(logicalInput);
            waitingForTickId = new(logicalInput.appliedAtTickId.tickId + 1);
        }

        public void Reset()
        {
            queue.Clear();
        }

        public bool HasInputForTickId(TickId tickId)
        {
            if (queue.Count == 0)
            {
                return false;
            }

            var firstTickId = queue.Peek().appliedAtTickId.tickId;
            var lastTick = waitingForTickId.tickId - 1;

            return tickId.tickId >= firstTickId && tickId.tickId <= lastTick;
        }


        public void DiscardUpToAndExcluding(TickId tickId)
        {
            while (queue.Count > 0)
            {
                if (queue.Peek().appliedAtTickId.tickId < tickId.tickId)
                {
                    queue.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        public LogicalInput Dequeue()
        {
            return queue.Dequeue();
        }
    }
}