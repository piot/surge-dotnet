/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public struct OrderedDatagramsOut
    {
        public OrderedDatagramsOut(byte initialValue)
        {
            Value = initialValue;
        }

        public OrderedDatagramsOut Next()
        {
            return new OrderedDatagramsOut((byte)(Value + 1));
        }
        public byte Value { get; }
    }
}