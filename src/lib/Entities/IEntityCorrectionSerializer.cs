/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Entities
{
    public interface IEntityCorrectionSerializer
    {
        /// <summary>
        ///     Serializes complete correction state in the entity.
        /// </summary>
        /// <param name="writer"></param>
        public void SerializeCorrectionState(IOctetWriter writer);
    }
}