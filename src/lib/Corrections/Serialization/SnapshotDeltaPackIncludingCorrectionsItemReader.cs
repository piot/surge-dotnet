/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections.Serialization
{
    public static class SnapshotDeltaPackIncludingCorrectionsItemReader
    {
        public static SnapshotDeltaPackIncludingCorrectionsItem Read(IOctetReader reader)
        {
            var isSet = reader.ReadUInt8();
            TickId? previousTickId;

            if (isSet == 0)
            {
                previousTickId = null;
            }
            else
            {
                previousTickId = TickIdReader.Read(reader);
            }

            var pack = SnapshotDeltaPackIncludingCorrectionsReader.Read(reader);

            return new(pack, previousTickId);
        }
    }
}