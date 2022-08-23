/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Snapshot;

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

        private bool isInitialized;
        private TickId waitingForTickId;

        public LogicalInput[] Collection => queue.ToArray();

        public TickId WaitingForTickId => waitingForTickId;

        public void AddLogicalInput(LogicalInput logicalInput)
        {
            if (!isInitialized)
            {
                waitingForTickId = logicalInput.appliedAtTickId;
                isInitialized = true;
            }

            if (waitingForTickId.tickId != logicalInput.appliedAtTickId.tickId)
            {
                throw new Exception(
                    $"wrong frame id for logical input. expected {waitingForTickId} but received {logicalInput.appliedAtTickId}");
            }

            queue.Enqueue(logicalInput);
            waitingForTickId = new(logicalInput.appliedAtTickId.tickId + 1);
        }

        public void DiscardUpToAndIncluding(TickId tickId)
        {
            while (queue.Count > 0)
            {
                if (queue.Peek().appliedAtTickId.tickId <= tickId.tickId)
                {
                    queue.Dequeue();
                }
            }
        }

        public LogicalInput Dequeue()
        {
            return queue.Dequeue();
        }
    }
}