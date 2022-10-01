/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Pulse.Client
{
    public static class PredictionStateChecksum
    {
        public static bool IsEqual(uint expectedFnvChecksum, ReadOnlySpan<byte> expectedPayload,
            ReadOnlySpan<byte> encounteredPayload, uint encounteredChecksum)
        {
            var checksumCompareEqual = encounteredChecksum == expectedFnvChecksum &&
                                       expectedPayload.Length == encounteredPayload.Length;
#if DEBUG
            var octetCompareEqual = encounteredPayload.SequenceEqual(expectedPayload);
            if (checksumCompareEqual != octetCompareEqual)
            {
                throw new("internal error, checksum compare and octet compare is not the same");
            }
#endif

            return checksumCompareEqual;
        }
    }
}