/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Pulse.Client
{
    public struct CorrectionInfo
    {
        public bool wasCreatedNow;
        public LocalPlayerInput localPlayerInput;

        public Memory<byte> payload;
    }
}