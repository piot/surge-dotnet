/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class CorrectionsHeaderReader
    {
        public static (EntityId, LocalPlayerIndex, ushort) Read(IOctetReader reader)
        {
            var targetEntityId = EntityIdReader.Read(reader);
            var localPlayerIndexReader = LocalPlayerIndexReader.Read(reader);
            var octetCount = reader.ReadUInt16();

            return (targetEntityId, localPlayerIndexReader, octetCount);
        }
    }
}