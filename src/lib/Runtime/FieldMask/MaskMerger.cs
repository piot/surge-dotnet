/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;

namespace Piot.Surge.FieldMask
{
    public static class MaskMerger
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong MergeBits(ulong first, ulong second)
        {
#if DEBUG
            if ((first & ChangedFieldsMask.DeletedMaskBit) != 0)
            {
                if ((second & ChangedFieldsMask.DeletedMaskBit) == 0)
                {
                    return second;
                }
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