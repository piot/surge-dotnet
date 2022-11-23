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
    public static class ClientPredictorHeaderWriter
    {
        public static void Write(EntityId targetEntityId, LocalPlayerIndex localPlayerIndex, IOctetWriter writer)
        {
            EntityIdWriter.WriteOctets(writer, targetEntityId);
            LocalPlayerIndexWriter.Write(localPlayerIndex, writer);
        }
    }
}