/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    public static class EntityCountReader
    {
        public static uint ReadEntityCount(IOctetReader reader)
        {
            var count = reader.ReadUInt16();
            if (count > 128)
            {
                throw new Exception($"suspicious count {count}");
            }

            return count;
        }
    }
}