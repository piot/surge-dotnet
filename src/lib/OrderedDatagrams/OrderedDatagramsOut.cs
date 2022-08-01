/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public class OrderedDatagramsOut
    {
        public byte SequenceId { get; private set; }

        public void Write(IOctetWriter writer)
        {
            writer.WriteUInt8(SequenceId++);
        }
    }
}