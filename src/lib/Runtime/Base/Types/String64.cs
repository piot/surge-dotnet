/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/



using System;
using System.Text;

namespace Surge.Types
{
    public struct String64
    {
        public readonly byte[] octets;

        public String64(string s)
        {
            octets = new byte[64];

            var stringOctets = Encoding.UTF8.GetBytes(s);

            octets[0] = (byte)stringOctets.Length;
            Array.Copy(stringOctets, 0, octets, 1, stringOctets.Length);
            for (var i = stringOctets.Length + 1; i < 64; ++i)
            {
                octets[i] = 0;
            }
        }

        public override string ToString()
        {
            var length = octets[0];
            var target = new byte[length];
            Array.Copy(octets, 1, target, 0, length);

            return $"{Encoding.UTF8.GetString(target)}";
        }
    }
}