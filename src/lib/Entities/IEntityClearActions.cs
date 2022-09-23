/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public interface IEntityClearActions
    {
        /// <summary>
        ///     Clearing all produced actions.
        /// </summary>
        public void ClearActions();
    }
}