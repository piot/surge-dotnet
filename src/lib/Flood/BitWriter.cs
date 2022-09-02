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
        private int octetPosition;

        public BitWriter(uint octetSize)
        {
            array = new byte[octetSize];
        }

        public uint BitPosition => (uint)bitPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(uint bits, int bitCount)
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
#if DEBUG
            if (bits != (bits & mask))
            {
                throw new ArgumentOutOfRangeException(nameof(bits),
                    $"there are bits set above the bit count {bits} {bitCount}");
            }
#endif

            accumulator |= (bits & mask) << bitsInAccumulator;

            bitsInAccumulator += bitCount;
            if (bitsInAccumulator > 32)
            {
                BinaryPrimitives.WriteUInt32BigEndian(array.Span.Slice(octetPosition, 4),
                    (uint)(accumulator & 0xffffffff));
                octetPosition += 4;
                accumulator >>= 32;
                bitsInAccumulator -= 32;
            }

            bitPosition += bitCount;
        }

        public ReadOnlySpan<byte> Close(out int outBitPosition)
        {
            if (bitsInAccumulator > 0)
            {
                BinaryPrimitives.WriteUInt32BigEndian(array.Span.Slice(octetPosition, 4),
                    (uint)(accumulator & 0xffffffff));
                octetPosition += 4;
            }

            outBitPosition = bitPosition;
            return array.Span[..octetPosition];
        }
    }
}