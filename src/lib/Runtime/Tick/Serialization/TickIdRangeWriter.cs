/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.Tick.Serialization
{
    public static class TickIdRangeWriter
    {
        /// <summary>
        ///     Writes a snapshot ID range to the stream.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tickIdRange"></param>
        public static void Write(IOctetWriter writer, TickIdRange tickIdRange)
        {
            TickIdWriter.Write(writer, tickIdRange.lastTickId);

            var count = 0u;
            if (tickIdRange.startTickId.tickId != 0)
            {
                count = (uint)((int)tickIdRange.lastTickId.tickId - (int)tickIdRange.startTickId.tickId);
                count = tickIdRange.Length;
#if DEBUG
                if (count > 255)
                {
                    throw new InvalidOperationException($"range is too big {tickIdRange} for serialization");
                }
#endif
            }

            writer.WriteUInt8((byte)count);
        }
    }
}