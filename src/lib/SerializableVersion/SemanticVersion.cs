/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Text;

namespace Piot.SerializableVersion
{
    public readonly struct SemanticVersion
    {
        public readonly ushort major;
        public readonly ushort minor;
        public readonly ushort patch;
        public readonly string suffix;

        static bool IsOnlyAsciiString(string str)
        {
            return Encoding.ASCII.GetByteCount(str) == str.Length;
        }

        public SemanticVersion(ushort major, ushort minor, ushort patch)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            suffix = string.Empty;
        }

        public SemanticVersion(ushort major, ushort minor, ushort patch, string suffix)
        {
            if (!IsOnlyAsciiString(suffix))
            {
                throw new ArgumentOutOfRangeException(nameof(suffix), "only ascii strings are allowed");
            }

            if (suffix.Length > 20)
            {
                throw new ArgumentOutOfRangeException(nameof(suffix), "only allowed 20 characters in suffix");
            }

            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.suffix = suffix;
        }

        public bool IsEqualDisregardSuffix(SemanticVersion other)
        {
            return major == other.major && minor == other.minor && patch == other.patch;
        }

        public override string ToString()
        {
            return $"[Version {major}.{minor}.{patch}{suffix}]";
        }
    }
}