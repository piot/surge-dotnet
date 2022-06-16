/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public static class EntityIdWriter
    {
        public static void Write(IOctetWriter writer, EntityId id)
        {
            writer.WriteUInt64(id.Value);
        }
    }
}