/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;
using Piot.Surge.Types;

namespace Piot.Surge.TypeSerialization
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
    }
}