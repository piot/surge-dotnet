/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public sealed class RollbackStack : SnapshotPackStack
    {
        public int Count => base.Count;

        public void PushUndoPack(TickId tickId, ReadOnlySpan<byte> payload)
        {
            if (tickId.tickId == 0)
            {
                throw new("must be wrong");
            }

            Push(tickId, payload);
        }

        public ReadOnlySpan<byte> GetUndoPackFromTickId(TickId tickId)
        {
            return GetPackFromTickId(tickId).payload.Span;
        }

        public override string ToString()
        {
            var s = "[RollbackStack ";

            if (stack.Count > 0)
            {
                s += $" top: {PeekTickId()}";
                s += $" bottom: {stack.PeekBottom().tickId}";
            }

            s += "]";

            return s;
        }
    }
}