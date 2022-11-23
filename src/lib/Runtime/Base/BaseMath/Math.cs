/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Maths
{
    public static class BaseMath
    {
        public static float Modulus(float x, float y)
        {
            // Alternative code, but I think it is slower
            // var r = x % y;
            // return r < 0 ? r + y : r;
            return (x % y + y) % y;
        }

        public static int Modulus(int x, int y)
        {
            // Alternative code, but I think it is slower
            // var r = x % y;
            // return r < 0 ? r + y : r;
            return (x % y + y) % y;
        }
    }
}