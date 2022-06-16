/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Snapshot;

namespace Piot.Surge.LogicalInput
{
    public class LogicalInputQueue
    {
        private readonly Queue<LogicalInput> queue = new();
        private SnapshotId lastFrameId;

        public LogicalInput[] Collection => queue.ToArray();

        public void AddLogicalInput(LogicalInput logicalInput)
        {
            if (lastFrameId.frameId >= logicalInput.appliedAtSnapshotId.frameId)
                throw new Exception("wrong frame id for logical input");
            queue.Enqueue(logicalInput);
            lastFrameId = logicalInput.appliedAtSnapshotId;
        }

        public void DiscardUpToAndIncluding(SnapshotId snapshotId)
        {
            while (queue.Count > 0)
                if (queue.Peek().appliedAtSnapshotId.frameId <= snapshotId.frameId)
                    queue.Dequeue();
        }

        public LogicalInput Dequeue()
        {
            return queue.Dequeue();
        }
    }
}