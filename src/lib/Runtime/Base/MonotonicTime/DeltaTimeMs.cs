/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public readonly struct DeltaTimeMs
    {
        public readonly uint ms;

        public DeltaTimeMs(uint ms)
        {
            this.ms = ms;
        }

        public override string ToString()
        {
            return $"[DeltaTimeMs: {ms}]";
        }
    }
}