/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Entities
{
    public interface IEntityBitDeserializer
    {
        public ulong Deserialize(IBitReader reader);
        public void DeserializeAll(IBitReader reader);

        public void DeserializeCorrectionState(IBitReader reader);
    }
}