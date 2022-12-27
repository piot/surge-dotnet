/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
#if NET48_OR_GREATER
    public sealed class MonotonicTimeEnvironmentMs : IMonotonicTimeMs
    {
        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is using the Environment tick count
        /// </summary>
        public TimeMs TimeInMs => new(Environment.TickCount64);
    }
#endif
}