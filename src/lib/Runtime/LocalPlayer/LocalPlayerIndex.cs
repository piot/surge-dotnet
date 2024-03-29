/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.LocalPlayer
{
    public struct LocalPlayerIndex
    {
        public byte Value;

        public LocalPlayerIndex(byte v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return $"[LocalPlayerIndex {Value}]";
        }
    }
}