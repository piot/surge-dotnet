/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public readonly struct Milliseconds
    {
        public readonly long ms;

        public Milliseconds(long ms)
        {
            this.ms = ms;
        }

        public override string ToString()
        {
            return $"[monotonic-ms: {ms}]";
        }

        public bool IsBeforeOrAt(Milliseconds other)
        {
            return ms <= other.ms;
        }
    }
}