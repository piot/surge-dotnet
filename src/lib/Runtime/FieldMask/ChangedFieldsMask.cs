/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.FieldMask
{
    /// <summary>
    ///     Holds a bit mask with a bit for each field in an Entity.
    /// </summary>
    public readonly struct ChangedFieldsMask
    {
        public const ulong DeletedMaskBit = 0x8000000000000000;

    }
}