/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public interface IEntityFireChanges
    {
        public void FireChanges(ulong changeMask);

        public void FireDestroyed();

        public void FireReplicate();
    }
}