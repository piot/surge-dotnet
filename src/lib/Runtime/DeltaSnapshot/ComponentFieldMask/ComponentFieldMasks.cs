/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public readonly struct ComponentChangedFieldMask
    {
        public ComponentChangedFieldMask(ulong mask)
        {
            this.mask = mask;
        }

        public readonly ulong mask;

        public override string ToString()
        {
            return $"[FieldChangeMask {mask:x4}]";
        }
    }

}