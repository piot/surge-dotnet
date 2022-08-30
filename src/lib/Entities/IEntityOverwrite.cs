/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public interface IEntityOverwrite
    {
        /// <summary>
        ///     Overwrites all detected field changes in the Entity as well as clearing all produced actions.
        /// </summary>
        public void Overwrite();
    }
}