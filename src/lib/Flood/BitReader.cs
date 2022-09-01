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
        private int bitPositionInAccumulator;
        private int uint64Position;

        public BitReader(ReadOnlySpan<byte> octets, int totalBitCount)
        {
            this.totalBitCount = totalBitCount;
            this.octets = octets.ToArray();
            if (octets.Length % 8 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(octets),
                    $"must have buffer in multiples of eight octets {octets.Length}");
            }

            var estimatedOctetCount = (totalBitCount + 7) / 8;
            var estimated64BitCount = (estimatedOctetCount + 7) / 8;
            var minimalOctetCount = estimated64BitCount * 8;

            if (octets.Length < minimalOctetCount)
            {
                throw new Exception(
                    $"wrong octets count compared to bit count, expected {minimalOctetCount} but received {octets.Length}");
            }

            bitPositionInAccumulator = 64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadBits(int bitCount)
        {
            bitPositionInAccumulator += bitCount;
            bitPosition += bitCount;
            if (bitPosition > totalBitCount)
            {
                throw new Exception($"reading more bits than is in buffer {bitCount} {bitPosition}/{totalBitCount}");
            }

            // Do we have all the bits requested in the accumulator?
            if (bitPositionInAccumulator < 64)
            {
                var mask = (1ul << bitCount) - 1;
                return (uint)((accumulator >> (64 - bitPositionInAccumulator)) & mask);
            }

            // Get the bits that was left in the accumulator
            bitPositionInAccumulator -= 64;
            var firstMask =
                (1ul << (bitCount - bitPositionInAccumulator)) - 1; // must check since shifting by >= 64 is undefined
            var v = (accumulator >> (bitPositionInAccumulator - bitCount)) & firstMask;

            // Set the accumulator with a new uint64 value
            accumulator = BinaryPrimitives.ReadUInt64BigEndian(octets.Span.Slice(uint64Position, 8));
            uint64Position += 8;

            // Get the bits we need from the newly read uint64 value
            var secondMask = (1ul << bitPositionInAccumulator) - 1;
            if (bitPositionInAccumulator > 0) // must check since shifting by >= 64 is undefined
            {
                v |= (accumulator >> (64 - bitPositionInAccumulator)) & secondMask;
            }

            return (uint)v;
        }
    }
}