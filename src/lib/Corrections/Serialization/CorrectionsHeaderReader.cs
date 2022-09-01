/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LocalPlayer.Serialization;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.Corrections.Serialization
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

        public static (EntityId, LocalPlayerIndex, ushort) Read(IBitReader reader)
        {
            var targetEntityId = EntityIdReader.Read(reader);
            var localPlayerIndexReader = LocalPlayerIndexReader.Read(reader);
            var octetCount = (ushort)reader.ReadBits(16);

            return (targetEntityId, localPlayerIndexReader, octetCount);
        }
    }
}