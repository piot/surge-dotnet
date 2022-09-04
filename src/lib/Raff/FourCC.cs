/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Text;

namespace Piot.Raff
{
    /// <summary>
    ///     Similar to https://en.wikipedia.org/wiki/FourCC
    /// </summary>
    public readonly struct FourCC
    {
        public FourCC(byte[] x)
        {
            if (x.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }

            Value = x;
        }

        public byte[] Value { get; }

        public static FourCC Make(string s)
        {
            if (s.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }

            var octets = Encoding.UTF8.GetBytes(s);
            if (octets.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }

            return new FourCC(octets);
        }
    }
}