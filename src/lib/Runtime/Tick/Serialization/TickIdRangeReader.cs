/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Tick.Serialization
{
    public static class TickIdRangeReader
    {
        /// <summary>
        ///     Reads a tick ID range. The range is always with ascending, consecutive IDs.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static TickIdRange Read(IOctetReader reader)
        {
            var currentId = TickIdReader.Read(reader);
            var numberOfTicks = reader.ReadUInt8();
            var startValue = 0u;

            if (numberOfTicks != 0)
            {
                startValue = (uint)((int)currentId.tickId - numberOfTicks + 1);
            }

            return new()
            {
                startTickId = new(startValue), lastTickId = currentId
            };
        }
    }
}