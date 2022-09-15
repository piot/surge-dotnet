/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Replay.Serialization
{
    public readonly struct CompleteStateEntry
    {
        public readonly ulong timeMs;
        public readonly uint tickId;
        public readonly ulong streamPosition;

        public CompleteStateEntry(ulong timeMs, uint tickId, ulong streamPosition)
        {
            this.timeMs = timeMs;
            this.tickId = tickId;
            this.streamPosition = streamPosition;
        }
    }
}