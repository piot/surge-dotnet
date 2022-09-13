/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;

namespace Piot.MonotonicTime
{
#if !UNITY_STANDALONE
    public sealed class MonotonicTimeEnvironmentMs : IMonotonicTimeMs
    {
        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is using the Environment tick count
        /// </summary>
        public Milliseconds TimeInMs => new(Environment.TickCount64);
    }
#endif
}