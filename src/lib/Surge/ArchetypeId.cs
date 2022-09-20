/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public readonly struct ArchetypeId
    {
        public readonly ushort id;

        public ArchetypeId(ushort id)
        {
            if (id == 0)
            {
                throw new("Illegal ID");
            }

            this.id = id;
        }

        public override string ToString()
        {
            return $"[ArchetypeId {id:X016} ]";
        }
    }
}