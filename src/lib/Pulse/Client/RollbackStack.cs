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
            Push(tickId, payload);
        }
    }
}