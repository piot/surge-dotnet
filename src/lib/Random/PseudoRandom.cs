/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Random
{
    public sealed class PseudoRandom : IRandom
    {
        private ulong value;

        public PseudoRandom(ulong seed)
        {
            value = seed;
        }

        public int Random(int max)
        {
            uint randomValue;

            (value, randomValue) = PseudoRandomNext.Next(value);

            return (int)(randomValue % (uint)max);
        }
    }
}