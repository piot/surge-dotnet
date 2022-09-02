/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Piot.Flood
{
    public class BitReader : IBitReader
    {
        private readonly ReadOnlyMemory<byte> octets;
        private readonly int totalBitCount;
        private ulong accumulator;
        private int bitPosition;
        private int bitsInAccumulator;
        private int octetPosition;

        public BitReader(ReadOnlySpan<byte> octets, int totalBitCount)
        {
            this.totalBitCount = totalBitCount;
            this.octets = octets.ToArray();
            if (octets.Length % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(octets),
                    $"must have buffer in multiples of eight octets {octets.Length}");
            }

            var estimatedOctetCount = (totalBitCount + 7) / 8;
            var estimated32BitCount = (estimatedOctetCount + 3) / 4;
            var minimalOctetCount = estimated32BitCount * 4;

            if (octets.Length < minimalOctetCount)
            {
                throw new Exception(
                    $"wrong octets count compared to bit count, expected {minimalOctetCount} but received {octets.Length}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadBits(int bitCount)
        {
#if DEBUG
            if (bitCount is < 1 or > 32)
            {
                throw new Exception("must read between 1-32 bits");
            }

            if (bitPosition + bitCount > totalBitCount)
            {
                throw new Exception("tried to read longer than buffer bit count");
            }
#endif

            if (bitsInAccumulator < bitCount)
            {
                accumulator |= BinaryPrimitives.ReadUInt32BigEndian(octets.Span.Slice(octetPosition, 4)) <<
                               bitsInAccumulator;
                octetPosition += 4;
                bitsInAccumulator += 32;
            }

            var mask = (1Lu << bitCount) - 1;
            var v = accumulator & mask;

            accumulator >>= bitCount;
            bitsInAccumulator -= bitCount;
            bitPosition += bitCount;

            return (uint)v;
        }
    }
}