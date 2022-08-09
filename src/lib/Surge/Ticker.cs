/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public static class Ticker
    {
        public static void Tick(IEntity[] entities)
        {
            foreach (var entity in entities)
            {
                entity.Tick();
            }
        }

        public static void Tick(IEntityContainer world)
        {
            Tick(world.AllEntities);
        }
    }
}