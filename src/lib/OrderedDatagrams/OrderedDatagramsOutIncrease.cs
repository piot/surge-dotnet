/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.OrderedDatagrams
{
    public sealed class OrderedDatagramsSequenceIdIncrease
    {
        private OrderedDatagramsSequenceId value;

        public OrderedDatagramsSequenceIdIncrease()
        {
        }

        public OrderedDatagramsSequenceIdIncrease(OrderedDatagramsSequenceId startValue)
        {
            value = startValue;
        }

        public OrderedDatagramsSequenceId Value => value;

        public void Increase()
        {
            value = value.Next();
        }
    }
}