/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public sealed class MonotonicTimeMockMs : IMonotonicTimeMs
    {
        public MonotonicTimeMockMs(TimeMs now)
        {
            TimeInMs = now;
        }

        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is mostly for testing.
        /// </summary>
        public TimeMs TimeInMs { get; set; }
    }
}