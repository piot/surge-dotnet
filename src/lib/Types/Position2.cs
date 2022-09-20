/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Types
{
    public struct Position2
    {
        public int x;
        public int y;

        public Position2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Position2 operator +(Position2 a, Velocity3 b)
        {
            return new()
            {
                x = a.x += b.x,
                y = a.y + b.y
            };
        }

        public static Position2 operator +(Position2 a, Position2 b)
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
            return obj != null && Equals((Position2)obj);
        }

        public static bool operator !=(Position2 a, Position2 b)
        {
            return a.x != b.x || a.y != b.y;
        }


        public static bool operator ==(Position2 a, Position2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public (float, float) ToFloats()
        {
            return (x / (float)Position3.MeterResolution,
                y / (float)Position3.MeterResolution);
        }

        public static Position2 FromFloats(float x, float y)
        {
            return new((int)(x * Position3.MeterResolution),
                (int)(y * Position3.MeterResolution));
        }

        public override string ToString()
        {
            var floats = ToFloats();
            return $"[pos2 {floats.Item1:n2}, {floats.Item2:n2}]";
        }
    }
}