/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public class RollbackQueue : TickIdPackQueue
    {
        public void EnqueueUndoPack(TickId tickId, ReadOnlySpan<byte> payload)
        {
            Enqueue(tickId, payload);
        }
    }
}