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

        public BitWriter(uint octetSize)
        {
            array = new byte[octetSize];
        }

        public uint BitPosition => (uint)bitPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(ulong bits, int bitCount)
        {
#if DEBUG
            if (bitCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "must be between 1 - 32 bits");
            }

            if (bitCount > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(bitCount), "only up to 32 bit supported");
            }
#endif

            var mask = (1Lu << bitCount) - 1;

            bitPosition += bitCount;
            bitsInAccumulator += bitCount;


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
                var bitCountThatIsLeftInAccumulator = 64 - bitsInAccumulator;
                var bitCountIntoNextAccumulator = bitCount = bitCountThatIsLeftInAccumulator;


                // Add parts of the bits to the existing accumulator, if needed
                var restBitsToAskFromBitsMask = (1ul << bitCountThatIsLeftInAccumulator) - 1;
                accumulator |= (bits >> bitCountThatIsLeftInAccumulator) & restBitsToAskFromBitsMask;

                BinaryPrimitives.WriteUInt64BigEndian(array.Span.Slice(uint64Position, 8), accumulator);
                uint64Position += 8;

                // Any bits that needs to spill over to the new accumulator                
                var secondMask = (1ul << bitCountIntoNextAccumulator) - 1;
                accumulator = (bits & secondMask) << (64 - bitsInAccumulator);
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