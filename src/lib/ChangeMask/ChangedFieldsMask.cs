/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.ChangeMask
{
    /// <summary>
    ///     Holds a bit mask with a bit for each field in an Entity.
    /// </summary>
    public struct ChangedFieldsMask
    {
        public ulong mask;

        public const ulong DeletedMaskBit = 0x8000000000000000;
        public const ulong AllFieldChangedMaskBits = 0x7fffffffffffffff;

        public ChangedFieldsMask(ulong mask)
        {
            this.mask = mask;
        }
    }
}