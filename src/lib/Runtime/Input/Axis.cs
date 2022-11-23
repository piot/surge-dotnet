/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Input
{
    public struct Axis
    {
        public short value;

        public const uint AxisFactor = 32767;
        public const uint AxisFactorF = AxisFactor;

        public override string ToString()
        {
            return $"[Axis {value}]";
        }

        public float ToFloat => Math.Clamp(value / AxisFactorF, -1.0f, 1.0f);

        public Axis(short v)
        {
            value = v;
        }

        public static Axis FromFloat(float v)
        {
            v = Math.Clamp(v, -1.0f, 1.0f);
            return new((short)(v * AxisFactorF));
        }
    }
}