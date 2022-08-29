/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Runtime.CompilerServices;

namespace Piot.Surge.ChangeMask
{
    public static class ChangedFieldsMerger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MergeBits(ulong first, ulong second)
        {
#if DEBUG
            if ((first & ChangedFieldsMask.DeletedMaskBit) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(first),
                    $"Can not merge anything into a previously deleted mask {first:08X} {second:08X}");
            }
#endif
            if (second == ChangedFieldsMask.DeletedMaskBit)
            {
                return second;
            }

            return first | second;
        }
    }
}