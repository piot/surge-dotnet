/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Stats;

namespace Piot.Transport.Stats
{
    public static class Constants
    {
        public const int CircularBufferSize = 128;
    }

    public struct TransportStatsInDirection
    {
        public Stat datagramCountPerSecond;
        public Stat datagramOctetSize;
        public Stat bitsPerSecond;
        public IEnumerable<int> datagramOctetSizes;

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