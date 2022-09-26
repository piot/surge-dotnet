/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.MonotonicTime
{
    public class OffsetTimeProvider : IMonotonicTimeMs
    {
        readonly IMonotonicTimeMs timeProvider;
        long deltaTimeMs;

        public OffsetTimeProvider(IMonotonicTimeMs timeProvider)
        {
            this.timeProvider = timeProvider;
        }

        public TimeMs TimeInMs => new(timeProvider.TimeInMs.ms + deltaTimeMs);

        public void AdjustTime(TimeMs timeNowMs)
        {
            deltaTimeMs = timeNowMs.ms - timeProvider.TimeInMs.ms;
        }
    }
}