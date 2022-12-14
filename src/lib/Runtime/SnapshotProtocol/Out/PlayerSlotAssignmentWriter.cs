/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Flood;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LocalPlayer.Serialization;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotProtocol.Out
{
    public class LocalPlayerAssignments
    {
        public EntityId entityToControl;
        public EntityId playerSlotEntity;
        public bool shouldPredict;

        public LocalPlayerAssignments()
        {
            
        }

        public LocalPlayerAssignments(EntityId playerSlotEntity)
        {
            this.playerSlotEntity = playerSlotEntity;
        }
    }
    
    public static class PlayerSlotAssignmentWriter
    {
        public static void Write(Dictionary<LocalPlayerIndex, LocalPlayerAssignments> playerSlotAssignmentForLocalPlayers, IBitWriter writer)
        {
            writer.WriteBits((byte)playerSlotAssignmentForLocalPlayers.Keys
                .Count, 3);

            foreach (var localPlayerAssignedPredictedEntity in playerSlotAssignmentForLocalPlayers)
            {
                LocalPlayerIndexWriter.Write(localPlayerAssignedPredictedEntity.Key, writer);
                
                var predictionEntity = localPlayerAssignedPredictedEntity.Value;
                EntityIdWriter.Write(writer, predictionEntity.playerSlotEntity);
                EntityIdWriter.Write(writer, predictionEntity.entityToControl);
                writer.WriteBits(predictionEntity.shouldPredict ? 1U : 0U, 1);
            }
        }
    }
}