/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class SnapshotDeltaPackIncludingCorrectionsItemWriter
    {
        public static void Write(SnapshotDeltaPackIncludingCorrectionsItem item, IOctetWriter writer)
        {
            if (item.previousTickId is null)
            {
                writer.WriteUInt8(0);
            }
            else
            {
                writer.WriteUInt8(1);
                TickIdWriter.Write(writer, item.previousTickId.Value);
            }

            SnapshotDeltaPackIncludingCorrectionsWriter.Write(item.Pack, writer);
        }
    }
}