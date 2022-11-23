/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Event
{
    public struct EventStreamPackItem
    {
        public TickId tickId;
        public EventSequenceId eventSequenceId;
        public Memory<byte> payload;
        public uint bitCount;
    }
}