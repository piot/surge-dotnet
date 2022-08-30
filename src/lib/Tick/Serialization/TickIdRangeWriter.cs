/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;

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
            writer.WriteUInt8(
                (byte)(tickIdRange.lastTickId.tickId - tickIdRange.startTickId.tickId));
        }
    }
}