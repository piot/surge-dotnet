/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Input;

namespace Piot.Surge.Types.Serialization
{
    public static class AxisWriter
    {
        public static void Write(Axis axis, IOctetWriter writer)
        {
            writer.WriteInt16(axis.value);
        }

        public static void Write(Axis axis, IBitWriter writer)
        {
            BitWriterUtils.WriteSignedBits(writer, axis.value, 16);
        }
    }
}