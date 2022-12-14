/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class AxisReader
    {
        public static void Read(IBitReader reader, ref Axis axis)
        {
            axis.value = (short)BitReaderUtils.ReadSignedBits(reader, 16);
        }
    }
}