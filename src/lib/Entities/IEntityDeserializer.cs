/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Entities
{
    public interface IEntityDeserializer
    {
        public ulong Deserialize(IOctetReader reader);
        public void DeserializeAll(IOctetReader reader);

        public void DeserializeCorrectionState(IOctetReader reader);
    }
}