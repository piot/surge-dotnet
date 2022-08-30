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
    public static class CorrectionsHeaderWriter
    {
        public static void Write(EntityId targetEntityId, LocalPlayerIndex localPlayerIndex, ushort octetCount,
            IOctetWriter writer)
        {
            EntityIdWriter.Write(writer, targetEntityId);
            LocalPlayerIndexWriter.Write(localPlayerIndex, writer);
            writer.WriteUInt16(octetCount);
        }
    }
}