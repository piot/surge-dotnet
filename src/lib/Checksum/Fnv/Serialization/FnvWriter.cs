/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class FnvWriter
    {
        public static void Write(IOctetWriter writer, uint fnv)
        {
            writer.WriteUInt32(fnv);
        }
    }
}