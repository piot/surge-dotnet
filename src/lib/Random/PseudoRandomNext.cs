/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Random
{
    public static class PseudoRandomNext
    {
        /// <summary>
        ///     Very simplistic pseudo random number generator.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static (ulong, uint) Next(ulong x)
        {
            x ^= x << 13;
            x ^= x >> 7;
            x ^= x << 17;

            return (x, (uint)((x >> 32) & 0x7FFFFFFF));
        }
    }
}