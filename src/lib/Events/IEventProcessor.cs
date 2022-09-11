/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Event
{
    public interface IEventProcessor
    {
        public void ReadAndApply(IBitReader bitReader);
        public void SkipOneEvent(IBitReader bitReader);
    }
}