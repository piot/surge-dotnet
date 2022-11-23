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
        public static void Write(IBitWriter writer, Axis axis)
        {
            BitWriterUtils.WriteSignedBits(writer, axis.value, 16);
        }
    }
}