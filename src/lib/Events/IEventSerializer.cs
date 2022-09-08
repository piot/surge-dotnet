/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event
{
    public interface IEventSerializer
    {
        public void Serialize(IBitWriter bitWriter);
        public void DeSerialize(IBitReader bitReader);
        public void Serialize(IOctetWriter octetWriter);
        public void DeSerialize(IOctetReader octetReader);
    }
}