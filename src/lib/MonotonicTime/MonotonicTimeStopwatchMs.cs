/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Diagnostics;

namespace Piot.MonotonicTime
{
    public class MonotonicTimeMs : IMonotonicTimeMs
    {
        /// <summary>
        ///     Returns the monotonic time in milliseconds. Implementation is using the Environment tick count
        /// </summary>
        public Milliseconds TimeInMs => new() { ms = Stopwatch.GetTimestamp() * 1000 / Stopwatch.Frequency };
    }
}