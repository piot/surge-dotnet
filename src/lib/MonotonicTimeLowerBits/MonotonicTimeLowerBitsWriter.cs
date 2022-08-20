/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class MonotonicTimeLowerBitsWriter
    {
        public static void Write(MonotonicTimeLowerBits lowerBits, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)(lowerBits.lowerBits & 0xffff));
        }
    }
}