/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public static class ContainerReset
    {
        public static void Reset(IEntityContainer world, INotifyWorldReset worldReset,
            INotifyEntityCreation notifyEntityCreation)
        {
            // Pretend that all entities died
            foreach (var entity in world.AllEntities)
            {
                entity.CompleteEntity.FireDestroyed();
            }

            //worldReset.NotifyGameEngineResetNetworkEntities();

            foreach (var entity in world.AllEntities)
            {
                notifyEntityCreation.CreateGameEngineEntity(entity);
            }
        }
    }
}