/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public readonly struct ComponentTypeId
    {
        public readonly ushort id;

        public static readonly ushort NoneValue = 0x00;
        public static ComponentTypeId None = new(NoneValue);

        public ComponentTypeId(ushort id)
        {

            this.id = id;
        }

        public override string ToString()
        {
            return $"[ComponentTypeId {id:X016} ]";
        }
    }
}