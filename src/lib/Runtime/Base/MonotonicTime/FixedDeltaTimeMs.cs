/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public readonly struct FixedDeltaTimeMs
    {
        public readonly uint ms;

        public FixedDeltaTimeMs(uint ms)
        {
            this.ms = ms;
        }

        public override string ToString()
        {
            return $"[FixedDeltaTime: {ms}]";
        }
    }
}