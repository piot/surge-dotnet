/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Types
{
    public struct Velocity2
    {
        public int x;
        public int y;

        public Velocity2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Position2 operator +(Velocity2 a, Velocity2 b)
        {
            return new()
            {
                x = a.x += b.x,
                y = a.y + b.y
            };
        }

        public bool Equals(Position2 other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && Equals((Velocity2)obj);
        }

        public static bool operator !=(Velocity2 a, Velocity2 b)
        {
            return a.x != b.x || a.y != b.y;
        }

        public static bool operator ==(Velocity2 a, Velocity2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public (float, float) ToFloats()
        {
            return (x / (float)Velocity3.VelocityResolution,
                y / (float)Velocity3.VelocityResolution);
        }

        public static Velocity2 FromFloats(float x, float y)
        {
            return new((int)(x * Velocity3.VelocityResolution),
                (int)(y * Velocity3.VelocityResolution));
        }

        public override string ToString()
        {
            var floats = ToFloats();
            return $"[vel2 {floats.Item1:n2}, {floats.Item2:n2}]";
        }
    }
}