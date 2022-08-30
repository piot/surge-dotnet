/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Stats;

namespace Piot.Transport.Stats
{
    public struct TransportStatsInDirection
    {
        public Stat datagramCountPerSecond;
        public Stat datagramOctetSize;
        public Stat bitsPerSecond;

        public override string ToString()
        {
            return
                $"[datagramCount:{datagramCountPerSecond} bandwidth:{bitsPerSecond} datagramOctetSize:{datagramOctetSize}]";
        }
    }

    public struct TransportStats
    {
        public TransportStatsInDirection send;
        public TransportStatsInDirection receive;

        public override string ToString()
        {
            return $"[\n  send:{send}\n  receive:{receive}\n]";
        }
    }
}