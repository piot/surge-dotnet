/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    ///     Holds the incoming delta snapshot received from the host to the client.
    /// </summary>
    public interface ISnapshotDeltaPackQueue
    {
        public int Count { get; }
        public void Enqueue(SnapshotDeltaPack pack);
        public SnapshotDeltaPack Dequeue();
    }
}