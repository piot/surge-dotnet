/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    /// <summary>
    ///     Very simple protocol to detect out of order and dropped datagrams.
    /// </summary>
    public sealed class OrderedDatagramsInChecker
    {
        bool hasReceivedInitialValue;

        public OrderedDatagramsInChecker()
        {
        }

        public OrderedDatagramsInChecker(OrderedDatagramsSequenceId specificValue)
        {
            hasReceivedInitialValue = true;
            LastValue = specificValue;
        }

        public OrderedDatagramsSequenceId LastValue { get; private set; } = new(0xff);

        public OrderedDatagramsSequenceId DebugLastReadValue { get; private set; } = new(0xff);

        public bool ReadAndCheck(IOctetReader reader)
        {
            var readValue = OrderedDatagramsSequenceIdReader.Read(reader);
#if DEBUG
            DebugLastReadValue = readValue;
#endif
            if (!hasReceivedInitialValue)
            {
                LastValue = readValue;
                hasReceivedInitialValue = true;
                return true;
            }

            var wasOk = readValue.IsValidSuccessor(LastValue);
            if (wasOk)
            {
                LastValue = readValue;
            }

            return wasOk;
        }

        public void Serialize(IOctetWriter writer)
        {
            writer.WriteUInt8((byte)(hasReceivedInitialValue ? 0x01 : 0x00));
            writer.WriteUInt8(LastValue.Value);
        }

        public void Deserialize(IOctetReader reader)
        {
            hasReceivedInitialValue = reader.ReadUInt8() != 0;
            LastValue = new(reader.ReadUInt8());
        }

        public override string ToString()
        {
            return $"[OrderedDatagramsInCheck {LastValue} ({hasReceivedInitialValue})]";
        }
    }
}