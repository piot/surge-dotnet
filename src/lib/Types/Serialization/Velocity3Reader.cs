/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity3Reader
    {
        public static Velocity3 Read(IOctetReader reader)
        {
            return new Velocity3
            {
                x = reader.ReadInt16(),
                y = reader.ReadInt16(),
                z = reader.ReadInt16()
            };
        }

        public static Velocity3 Read(IBitReader reader)
        {
            return new Velocity3
            {
                x = (short)reader.ReadBits(16),
                y = (short)reader.ReadBits(16),
                z = (short)reader.ReadBits(16)
            };
        }
    }
}