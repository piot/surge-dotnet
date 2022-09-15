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
            OctetMarker.AssertMarker(reader, Constants.MonotonicTimeLowerBitsSync);
#endif
            return new(reader.ReadUInt16());
        }
    }
}