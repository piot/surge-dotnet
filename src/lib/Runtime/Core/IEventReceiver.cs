/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



namespace Piot.Surge.Core
{
    public interface IEventReceiver
    {
        public void ReceiveEvent<T>(in T eventData) where T : struct;
    }
}