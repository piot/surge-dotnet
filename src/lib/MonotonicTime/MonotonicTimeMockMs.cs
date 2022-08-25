/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public class MonotonicTimeMockMs : IMonotonicTimeMs
    {
        public MonotonicTimeMockMs(Milliseconds now)
        {
            TimeInMs = now;
        }

        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is mostly for testing.
        /// </summary>
        public Milliseconds TimeInMs { get; set; }
    }
}