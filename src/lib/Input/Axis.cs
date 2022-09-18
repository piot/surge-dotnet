/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Input
{
    public readonly struct Axis
    {
        public readonly short value;

        public override string ToString()
        {
            return $"[Axis {value}]";
        }

        public float ToFloat => value / 32768.0f;
    }
}