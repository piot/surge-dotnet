/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public interface IEntityDeserializer
    {
        public void Deserialize(ulong serializeFlags, IOctetReader reader);
        public void DeserializeAll(IOctetReader reader);
    }
}