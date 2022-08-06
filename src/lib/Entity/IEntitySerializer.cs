/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge
{
    public interface IEntitySerializer
    {
        /**
         * Serializes the fields in the @param changedFieldsMask.
         */
        public void Serialize(ulong changedFieldsMask, IOctetWriter writer);
        /**
         * Serializes all fields in the entity. Useful for handling late join / rejoin
         * where no parts of the data is known beforehand.
         */
        public void SerializeAll(IOctetWriter writer);
    }
}