/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public struct EntityId
    {
        public ulong Value;

        public EntityId(ulong value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"[entityId {Value}]";
        }
    }
}