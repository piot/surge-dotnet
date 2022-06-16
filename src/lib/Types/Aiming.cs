/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Numerics;

namespace Piot.Surge.Types
{
    public struct Aiming
    {
        public ushort yaw;
        public short pitch;

        public float Yaw => yaw * (float)Math.PI * 2.0f / 0xffff;
        public float Pitch => pitch * (float)Math.PI / 32768;


        public bool Equals(Aiming other)
        {
            return yaw == other.yaw && pitch == other.pitch;
        }

        public override bool Equals(object? obj)
        {
            return obj != null && Equals((Aiming)obj);
        }

        public static bool operator !=(Aiming a, Aiming b)
        {
            return a.yaw != b.yaw || a.pitch != b.pitch;
        }


        public static bool operator ==(Aiming a, Aiming b)
        {
            return a.yaw == b.yaw && a.pitch == b.pitch;
        }

        public Aiming(ushort yaw, short pitch)
        {
            this.yaw = yaw;
            this.pitch = pitch;
        }

        public Vector3 ToDirection
        {
            get
            {
                var pitch = Pitch;
                var yaw = Yaw;

                return new Vector3(MathF.Cos(pitch) * MathF.Sin(yaw), MathF.Sin(pitch),
                    MathF.Cos(pitch) * MathF.Cos(yaw));
            }
        }
    }
}