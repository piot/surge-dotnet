/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge
{
    public interface IEntitySerializer
    {
        public void Serialize(ulong serializeFlags, IOctetWriter writer);
        public void SerializeAll(IOctetWriter writer);
    }
}