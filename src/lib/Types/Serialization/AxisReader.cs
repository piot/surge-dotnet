/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Input;

namespace Piot.Surge.Types.Serialization
{
    public static class AxisReader
    {
        public static Axis Read(IOctetReader reader)
        {
            var v = reader.ReadInt16();

            return new(v);
        }

        public static Axis Read(IBitReader reader)
        {
            var v = (short)BitReaderUtils.ReadSignedBits(reader, 16);

            return new(v);
        }
    }
}