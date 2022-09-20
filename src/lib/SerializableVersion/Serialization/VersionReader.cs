/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Text;
using Piot.Flood;

namespace Piot.SerializableVersion.Serialization
{
    public static class VersionReader
    {
        static string ReadString(IOctetReader reader)
        {
            var octetLength = reader.ReadUInt8();
            if (octetLength > 20)
            {
                throw new("illegal length");
            }

            var octets = reader.ReadOctets(octetLength);
            return Encoding.ASCII.GetString(octets);
        }

        public static SemanticVersion Read(IOctetReader reader)
        {
            return new(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), ReadString(reader));
        }
    }
}