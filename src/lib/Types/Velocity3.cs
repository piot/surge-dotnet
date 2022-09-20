/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Types
{
    public struct Velocity3
    {
        public int x;
        public int y;
        public int z;

        internal const int VelocityResolution = 16;

        public Velocity3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Position3 operator +(Velocity3 a, Velocity3 b)
        {
            return new()
            {
                x = a.x += b.x,
                y = a.y + b.y,
                z = a.z + b.z
            };
        }

        public readonly bool Equals(Position3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && Equals((Velocity3)obj);
        }

        public static bool operator !=(Velocity3 a, Velocity3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }


        public static bool operator ==(Velocity3 a, Velocity3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public (float, float, float) ToFloats()
        {
            return (x / (float)VelocityResolution,
                y / (float)VelocityResolution,
                z / (float)VelocityResolution);
        }

        public static Velocity3 FromFloats(float x, float y, float z)
        {
            return new((int)(x * VelocityResolution),
                (int)(y * VelocityResolution),
                (int)(z * VelocityResolution));
        }

        public override string ToString()
        {
            var floats = ToFloats();
            return $"[vel3 {floats.Item1:n2}, {floats.Item2:n2}, {floats.Item3:n2}]";
        }
    }
}