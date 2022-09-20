/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Numerics;

namespace Piot.Surge.Types
{
    public struct UnitVector3
    {
        public int x;
        public int y;
        public int z;

        internal const int UnitResolution = 256;

        public UnitVector3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static UnitVector3 FromFloats(float x, float y, float z)
        {
            return new((int)(x * UnitResolution),
                (int)(y * UnitResolution),
                (int)(z * UnitResolution));
        }

        public Vector2 xz => new(x / (float)UnitResolution, z / (float)UnitResolution);
    }
}