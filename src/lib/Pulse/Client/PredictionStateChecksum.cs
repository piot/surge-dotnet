/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Collections;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionStateChecksum
    {
        public static bool IsEqual(uint expectedFnvChecksum, int expectedPayloadLength, ReadOnlySpan<byte> expectedPayload, ReadOnlySpan<byte> encounteredPayload)
        {
            var encounteredChecksum = Fnv.Fnv.ToFnv(encounteredPayload);
            var checksumCompareEqual = encounteredChecksum == expectedFnvChecksum && expectedPayloadLength == encounteredPayload.Length;
            var octetCompareEqual = encounteredPayload.SequenceEqual(expectedPayload);
            if (checksumCompareEqual != octetCompareEqual)
            {
                throw new("internal error, checksum compare and octet compare is not the same");
            }

            return octetCompareEqual;
        }
    }
}