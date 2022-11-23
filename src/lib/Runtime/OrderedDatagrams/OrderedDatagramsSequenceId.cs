/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.OrderedDatagrams
{
    /// <summary>
    ///     Very simple protocol to detect out of order and dropped datagrams.
    /// </summary>
    public readonly struct OrderedDatagramsSequenceId
    {
        public OrderedDatagramsSequenceId(byte sequenceId)
        {
            Value = sequenceId;
        }

        public OrderedDatagramsSequenceId Next => new((byte)(Value + 1));

        public byte Value { get; }

        public bool IsValidSuccessor(OrderedDatagramsSequenceId value)
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
                throw new("delta is negative");
            }

            return diff > 0 && diff <= 127;
        }

        public override string ToString()
        {
            return $"[OrderedDatagramsSequenceId {Value}]";
        }
    }
}