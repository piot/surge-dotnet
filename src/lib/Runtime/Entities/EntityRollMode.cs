/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public struct RollMode
    {
        public EntityRollMode mode;
    }

    public enum EntityRollMode
    {
        Rollback,
        Rollforth,
        Replicate,
        Predict
    }
}