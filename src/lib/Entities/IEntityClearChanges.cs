/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public interface IEntityClearChanges
    {
        /// <summary>
        ///     Copies current state to previous as well as clearing all produced actions.
        /// </summary>
        public void ClearChanges();
    }
}