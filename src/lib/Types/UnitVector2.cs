/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Types
{
    public struct UnitVector2
    {
        public int x;
        public int y;

        internal const int UnitResolution = 256;

        public UnitVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static UnitVector2 FromFloats(float x, float y)
        {
            return new((int)(x * UnitResolution),
                (int)(y * UnitResolution));
        }
    }
}