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
            return new MonotonicTimeLowerBits { lowerBits = reader.ReadUInt16() };
        }
    }
}