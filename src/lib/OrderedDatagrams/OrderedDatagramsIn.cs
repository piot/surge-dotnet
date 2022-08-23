/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.OrderedDatagrams
{
    /// <summary>
    ///     Very simple protocol to detect out of order and dropped datagrams.
    /// </summary>
    public readonly struct OrderedDatagramsIn
    {
        public OrderedDatagramsIn(byte sequenceId)
        {
            Value = sequenceId;
        }

        public byte Value { get; }

        public bool IsValidSuccessor(OrderedDatagramsIn value)
        {
            int diff;

            var id = value.Value;
            if (Value < id)
            {
                diff = Value + 256 - id;
            }
            else
            {
                diff = Value - id;
            }

            if (diff < 0)
            {
                throw new Exception("delta is negative");
            }

            return diff > 0 && diff <= 127;
        }

        public override string ToString()
        {
            return $"[OrderedDatagramsIn {Value}]";
        }
    }
}