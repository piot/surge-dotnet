/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public class OrderedDatagramsIn
    {
        private byte expectedSequenceId;

        private bool IsValidSuccessor(byte id)
        {
            int diff;

            if (id < expectedSequenceId)
                diff = id + 256 - expectedSequenceId;
            else
                diff = id - expectedSequenceId;

            if (diff < 0) throw new Exception("delta is negative");

            return diff <= 127;
        }

        public bool Read(IOctetReader reader)
        {
            var encounteredId = reader.ReadUInt8();
            var wasValid = IsValidSuccessor(encounteredId);
            if (wasValid) expectedSequenceId = (byte)(encounteredId + 1);

            return wasValid;
        }
    }
}