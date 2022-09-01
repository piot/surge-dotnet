/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Piot.Flood
{
    public class BitWriter : IBitWriter
    {
        private readonly Memory<byte> array;
        private ulong accumulator;
        private int bitPosition;
        private int bitsInAccumulator;
        private int uint64Position;

        public BitWriter(int octetSize)
        {
            array = new byte[octetSize];
        }

        public uint BitPosition => (uint)bitPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(ulong bits, int bitCount)
        {
#if DEBUG
            if (bitCount > 64)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "only up to 64 bit supported");
            }
#endif

            bitPosition += bitCount;
            bitsInAccumulator += bitCount;

            var mask = (1lu << bitCount) - 1;

#if DEBUG
            if (bits != (bits & mask))
            {
                throw new ArgumentOutOfRangeException(nameof(bits),
                    $"there are bits set above the bit count {bits} {bitCount}");
            }
#endif

            // Can we fit all bits into the accumulator?
            if (bitsInAccumulator < 64)
            {
                accumulator |= (bits & mask) << (64 - bitsInAccumulator);
            }
            else
            {
                bitsInAccumulator -= 64;

                // Add parts of the bits to the existing accumulator, if needed
                var firstMask = (1ul << (bitCount - bitsInAccumulator)) - 1;
                accumulator |= (bits >> bitsInAccumulator) & firstMask;

                BinaryPrimitives.WriteUInt64BigEndian(array.Span.Slice(uint64Position, 8), accumulator);
                uint64Position += 8;
                accumulator = 0;

                // Any bits that needs to spill over to the new accumulator                
                var secondMask = (1ul << bitsInAccumulator) - 1;
                accumulator |= (bits & secondMask) << (64 - bitsInAccumulator);
            }
        }

        public ReadOnlySpan<byte> Close(out int outBitPosition)
        {
            if (bitsInAccumulator > 0)
            {
                BinaryPrimitives.WriteUInt64BigEndian(array.Span.Slice(uint64Position, 8), accumulator);
                uint64Position += 8;
            }

            outBitPosition = bitPosition;
            return array.Span[..uint64Position];
        }
    }
}