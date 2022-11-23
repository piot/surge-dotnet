/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public static class OrderedDatagramsSequenceIdWriter
    {
        public static void Write(IOctetWriter writer, OrderedDatagramsSequenceId datagramsOut)
        {
            writer.WriteUInt8(datagramsOut.Value);
        }
    }
}