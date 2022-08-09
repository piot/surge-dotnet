/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.ChangeMask
{
    public struct ChangedFieldsMask
    {
        public ulong mask;

        public const ulong DeletedMaskBit = 0x8000000000000000;
        public const ulong AllFieldChangedMaskBits = 0x7fffffffffffffff;

        public ChangedFieldsMask(ulong mask)
        {
            this.mask = mask;
        }

        public static ulong MergeBits(ulong first, ulong second)
        {
            if ((first & DeletedMaskBit) != 0)
            {
                throw new Exception($"Can not merge previously deleted masks {first:08X} {second:08X}");
            }

            return first | second;
        }
    }
}