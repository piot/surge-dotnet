/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Types.Serialization
{
    public static class Velocity2Reader
    {
        public static Velocity2 Read(IOctetReader reader)
        {
            return new Velocity2
            {
                x = reader.ReadInt16(),
                y = reader.ReadInt16()
            };
        }
    }
}