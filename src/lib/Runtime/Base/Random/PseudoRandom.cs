/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Random
{
    public sealed class PseudoRandom : IRandom
    {
        ulong value;

        public PseudoRandom(ulong seed)
        {
            if (seed == 0)
            {
                throw new("seed can not be zero");
            }

            value = seed;
        }

        public int Random(int max)
        {
            if (max == 0)
            {
                return 0;
            }

            uint randomValue;

            (value, randomValue) = PseudoRandomNext.Next(value);

            return (int)(randomValue % (uint)max);
        }
    }
}