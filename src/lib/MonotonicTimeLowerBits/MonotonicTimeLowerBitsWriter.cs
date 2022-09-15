/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class Constants
    {
        public const byte MonotonicTimeLowerBitsSync = 0x64;
    }

    public static class MonotonicTimeLowerBitsWriter
    {
        public static void Write(MonotonicTimeLowerBits lowerBits, IOctetWriter writer)
        {
#if DEBUG
            OctetMarker.WriteMarker(writer, Constants.MonotonicTimeLowerBitsSync);
#endif

            writer.WriteUInt16((ushort)(lowerBits.lowerBits & 0xffff));
        }
    }
}