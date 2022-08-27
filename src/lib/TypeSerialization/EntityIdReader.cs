/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge
{
    public static class EntityIdReader
    {
        public static EntityId Read(IOctetReader reader)
        {
            return new EntityId(reader.ReadUInt16());
        }
    }
}