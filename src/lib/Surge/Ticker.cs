/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public static class Ticker
    {
        public static void Tick(IEntity[] entities, SimulationMode mode)
        {
            foreach (var entity in entities)
            {
                entity.Tick(mode);
            }
        }

        /// <summary>
        ///     Calls Tick() on all entities in the <paramref name="world" />.
        /// </summary>
        /// <param name="world"></param>
        public static void Tick(IEntityContainer world, SimulationMode mode)
        {
            Tick(world.AllEntities, mode);
        }
    }
}