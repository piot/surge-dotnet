/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text;

namespace Piot.Raff
{
    /// <summary>
    ///     Similar to https://en.wikipedia.org/wiki/FourCC
    /// </summary>
    public readonly struct FourCC
    {
        public uint Value { get; }

        public FourCC(byte[] x)
        {
            if (x.Length != 4)
            {
                throw new("FourCC must have exactly four octets");
            }

            Value = ((uint)x[0] << 24) | ((uint)x[1] << 16) | ((uint)x[2] << 8) | x[3];
        }

        public FourCC(uint v)
        {
            Value = v;
        }

        public static FourCC Make(string s)
        {
            if (s.Length != 4)
            {
                throw new("FourCC must have exactly four octets");
            }

            var octets = Encoding.ASCII.GetBytes(s);
            if (octets.Length != 4)
            {
                throw new("FourCC must have exactly four octets");
            }

            return new(octets);
        }
    }
}