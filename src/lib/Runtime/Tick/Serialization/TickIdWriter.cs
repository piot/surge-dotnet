/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Tick.Serialization
{
    public static class TickIdWriter
    {
        /// <summary>
        ///     Writes a tick ID to the stream writer.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="tickId"></param>
        public static void Write(IOctetWriter writer, TickId tickId)
        {
            writer.WriteUInt32(tickId.tickId);
        }
    }
}