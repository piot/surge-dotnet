/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LocalPlayer.Serialization;
using Piot.Surge.SnapshotProtocol.Out;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotProtocol.In
{
    public static class PlayerSlotAssignmentReader
    {
        public static Dictionary<LocalPlayerIndex, LocalPlayerAssignments> Read(IBitReader reader)
        {
            var count = reader.ReadBits(3);
            var localPlayerAssignmentDictionary = new Dictionary<LocalPlayerIndex, LocalPlayerAssignments>();
            for (var i=0; i<count;++i)
            {
                var localPlayerIndex = LocalPlayerIndexReader.Read(reader);
                var assignment = new LocalPlayerAssignments();
                EntityIdReader.Read(reader, out assignment.playerSlotEntity);
                EntityIdReader.Read(reader, out assignment.entityToControl);
                assignment.shouldPredict = reader.ReadBits(1) != 0;
                localPlayerAssignmentDictionary.Add(localPlayerIndex, assignment);
            }

            return localPlayerAssignmentDictionary;
        }
    }
}