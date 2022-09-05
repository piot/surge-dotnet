/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Types
{
    public struct Position3
    {
        public int x;
        public int y;
        public int z;

        internal const int MeterResolution = 512; // About 2 mm in precision and 4 million meters

        public Position3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Position3 operator +(Position3 a, Velocity3 b)
        {
            return new Position3
            {
                x = a.x += b.x,
                y = a.y + b.y,
                z = a.z + b.z
            };
        }

        public static Position3 operator +(Position3 a, Position3 b)
        {
            return new Position3
            {
                x = a.x += b.x,
                y = a.y + b.y,
                z = a.z + b.z
            };
        }

        public bool Equals(Position3 other)
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && Equals((Position3)obj);
        }

        public static bool operator !=(Position3 a, Position3 b)
        {
            return a.x != b.x || a.y != b.y || a.z != b.z;
        }


        public static bool operator ==(Position3 a, Position3 b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public (float, float, float) ToFloats()
        {
            return (x / (float)MeterResolution,
                y / (float)MeterResolution,
                z / (float)MeterResolution);
        }

        public static Position3 FromFloats(float x, float y, float z)
        {
            return new Position3((int)(x * MeterResolution),
                (int)(y * MeterResolution),
                (int)(z * MeterResolution));
        }

        public override string ToString()
        {
            var floats = ToFloats();
            return $"[pos3 {floats.Item1:n2}, {floats.Item2:n2}, {floats.Item3:n2}]";
        }
    }
}