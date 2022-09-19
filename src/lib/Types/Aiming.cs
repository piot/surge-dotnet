/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Types
{
    public struct Aiming
    {
        public ushort yaw;
        public short pitch;

        public readonly float Yaw => yaw * (float)Math.PI * 2.0f / 65535.0f;

        public readonly float YawDegrees => yaw * 360.0f / 65535.0f;
        public readonly float Pitch => pitch * PitchMax / 32768.0f;
        public readonly float PitchDegrees => pitch * 89.0f / 32768.0f;

        public const float PitchMax = (float)Math.PI / 2.0f - 0.1f;

        public UnitVector2 YawDirection => UnitVector2.FromFloats((float)Math.Cos(Yaw), (float)Math.Sin(Yaw));

        public static float RealMod(float x, float y)
        {
            return (x % y + y) % y;
        }

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

        public Aiming(float newYaw, float newPitch)
        {
            if (newPitch < -PitchMax)
            {
                newPitch = -PitchMax;
            }

            if (newPitch > PitchMax)
            {
                newPitch = PitchMax;
            }

            newYaw = RealMod(newYaw, 2.0f * (float)Math.PI);

            yaw = (ushort)(newYaw / (2.0f * Math.PI) * 65535.0f);
            pitch = (short)(newPitch * 32767.0 / PitchMax);
        }

        public UnitVector3 ToDirection
        {
            get
            {
                var pitchRad = Pitch;
                var yawRad = Yaw;

                return UnitVector3.FromFloats(MathF.Cos(pitchRad) * MathF.Sin(yawRad), MathF.Sin(pitchRad),
                    MathF.Cos(pitchRad) * MathF.Cos(yawRad));
            }
        }
    }
}