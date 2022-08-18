/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public interface ISimpleLogic
    {
        /// <summary>
        ///     Called to tick the logic for an entity
        ///     The Tick is not allowed to be passed any inputs, but must act from the fields stored in the implementing struct.
        /// </summary>
        public void Tick();
    }
}