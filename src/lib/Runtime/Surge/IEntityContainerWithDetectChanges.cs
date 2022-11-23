/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.CompleteSnapshot;
using Piot.Surge.DeltaSnapshot.ComponentFieldMask;

namespace Piot.Surge
{
    public interface IEntityContainerWithDetectChanges : IEcsContainer
    {
        public AllEntitiesChangesThisTick EntitiesThatHasChanged(ILog log);
    }
}