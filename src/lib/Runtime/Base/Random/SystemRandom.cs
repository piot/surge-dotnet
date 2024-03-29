/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Random
{
    public sealed class SystemRandom : IRandom
    {
        readonly System.Random rand = new();

        public int Random(int max)
        {
            return rand.Next(max);
        }
    }
}