/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge
{
    public interface IDetectEntityChanges
    {
        public IEntity[] Created { get; }
        public IEntity[] Deleted { get; }

        void ClearDelta();
    }
}