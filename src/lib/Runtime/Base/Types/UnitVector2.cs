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

        public UnitVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(UnitVector2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && Equals((UnitVector2)obj);
        }

        public static bool operator !=(UnitVector2 a, UnitVector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }


        public static bool operator ==(UnitVector2 a, UnitVector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static UnitVector2 FromFloats(float x, float y)
        {
            return new((int)(x * UnitVector3.UnitResolution),
                (int)(y * UnitVector3.UnitResolution));
        }

        public (float, float) ToFloats()
        {
            return (x / UnitVector3.UnitResolutionF, y / UnitVector3.UnitResolutionF);
        }

        public override string ToString()
        {
            var floats = ToFloats();
            return $"[UnitVector2 {floats.Item1}, {floats.Item2}]";
        }
    }
}