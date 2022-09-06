/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.GeneratedEntity;

namespace Piot.Surge
{
    public interface INotifyEntityCreation
    {
        public void NotifyCreation(IGeneratedEntity entity);
    }
}