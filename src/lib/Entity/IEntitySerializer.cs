/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge
{
    public interface IEntitySerializer
    {
        /// <summary>
        ///     Serializes the fields that have a bit set in <paramref name="changedFieldsMask" />.
        /// </summary>
        /// <param name="changedFieldsMask"></param>
        /// <param name="writer"></param>
        public void Serialize(ulong changedFieldsMask, IOctetWriter writer);

        /// <summary>
        ///     Serializes all fields in the entity. Useful for handling late join / rejoin
        ///     where no parts of the data is known beforehand.
        /// </summary>
        /// <param name="writer"></param>
        public void SerializeAll(IOctetWriter writer);
    }
}