/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.OrderedDatagrams
{
    public class OrderedDatagramsOutIncrease
    {
        private OrderedDatagramsOut value;

        public OrderedDatagramsOutIncrease()
        {
        }

        public OrderedDatagramsOutIncrease(OrderedDatagramsOut startValue)
        {
            value = startValue;
        }

        public OrderedDatagramsOut Value => value;

        public void Increase()
        {
            value = value.Next();
        }
    }
}