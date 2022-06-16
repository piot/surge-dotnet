/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public static class OverWriter
    {
        private static void Overwrite(IEntity[] entities)
        {
            foreach (var entity in entities) entity.Overwrite();
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Global
        public static void Overwrite(IEntityContainer world, EntityId[] ids)
        {
            foreach (var id in ids)
            {
                var entity = world.FetchEntity(id);
                entity.Overwrite();
            }
        }

        public static void Overwrite(IEntityContainer world)
        {
            Overwrite(world.AllEntities);
        }
    }
}