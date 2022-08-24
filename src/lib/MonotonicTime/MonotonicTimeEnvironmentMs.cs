/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.MonotonicTime
{
    public class MonotonicTimeEnvironmentMs : IMonotonicTimeMs
    {
        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is using the Environment tick count
        /// </summary>
        public Milliseconds TimeInMs => new(Environment.TickCount64);
    }
}