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
    public static class ClientPredictorHeaderReader
    {
        public static (EntityId, LocalPlayerIndex) Read(IBitReader reader)
        {
            var targetEntityId = new EntityId();
            EntityIdReader.Read(reader, out targetEntityId);
            var localPlayerIndexReader = LocalPlayerIndexReader.Read(reader);

            return (targetEntityId, localPlayerIndexReader);
        }
    }
}