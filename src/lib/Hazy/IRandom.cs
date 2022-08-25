/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Hazy
{
    public interface IRandom
    {
        int Random(int max);
    }

    public class SystemRandom : IRandom
    {
        private readonly Random rand = new();

        public int Random(int max)
        {
            return rand.Next(max);
        }
    }
}