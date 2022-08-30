/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class MonotonicTimeLowerBitsReader
    {
        public static MonotonicTimeLowerBits Read(IOctetReader reader)
        {
#if DEBUG
            if (reader.ReadUInt8() != Constants.MonotonicTimeLowerBitsSync)
            {
                throw new Exception("desync in monotonic time");
            }
#endif
            return new(reader.ReadUInt16());
        }
    }
}