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
        private bool hasReceivedInitialValue;

        public OrderedDatagramsInChecker()
        {
        }

        public OrderedDatagramsInChecker(OrderedDatagramsSequenceId specificValue)
        {
            hasReceivedInitialValue = true;
            LastValue = specificValue;
        }

        public OrderedDatagramsSequenceId LastValue { get; private set; } = new(0xff);

        public bool ReadAndCheck(IOctetReader reader)
        {
            var readValue = OrderedDatagramsSequenceIdReader.Read(reader);
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

        public override string ToString()
        {
            return $"[OrderedDatagramsInCheck {LastValue} ({hasReceivedInitialValue})]";
        }
    }
}