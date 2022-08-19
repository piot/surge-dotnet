/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public struct EntityId
    {
        public uint Value;

        public EntityId(uint value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"[entityId {Value}]";
        }
    }
}