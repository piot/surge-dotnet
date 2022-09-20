/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;

namespace Piot.Surge
{
    public static class ChangeClearer
    {
        static void ClearChanges(IEntity[] entities)
        {
            foreach (var entity in entities)
            {
                entity.CompleteEntity.ClearChanges();
            }
        }

        /// <summary>
        ///     Overwrites all detected field changes in the <paramref name="world" />.
        /// </summary>
        /// <param name="world"></param>
        public static void ClearChanges(IEntityContainer world)
        {
            ClearChanges(world.AllEntities);
        }

        /// <summary>
        ///     Overwrites all detected field changes in the <paramref name="world" />.
        /// </summary>
        /// <param name="world"></param>
        public static void OverwriteAuthoritative(IEntityContainerWithDetectChanges world)
        {
            world.ClearDelta();
            ClearChanges(world.AllEntities);
        }
    }
}