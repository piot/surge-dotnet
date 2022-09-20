/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Tick.Serialization
{
    public static class TickIdReader
    {
        /// <summary>
        ///     Reads a tick ID from the stream reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static TickId Read(IOctetReader reader)
        {
            return new(reader.ReadUInt32());
        }
    }
}