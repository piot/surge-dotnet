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
    public class OrderedDatagramsInChecker
    {
        private bool hasReceivedInitialValue;
        private OrderedDatagramsIn lastValue = new(0xff);

        public OrderedDatagramsInChecker()
        {
        }

        public OrderedDatagramsInChecker(OrderedDatagramsIn specificValue)
        {
            hasReceivedInitialValue = true;
            lastValue = specificValue;
        }

        public bool ReadAndCheck(IOctetReader reader)
        {
            var readValue = OrderedDatagramsInReader.Read(reader);
            if (!hasReceivedInitialValue)
            {
                lastValue = readValue;
                hasReceivedInitialValue = true;
                return true;
            }

            var wasOk = readValue.IsValidSuccessor(lastValue);
            if (wasOk)
            {
                lastValue = readValue;
            }

            return wasOk;
        }
    }
}