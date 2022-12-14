/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using Piot.Surge.LocalPlayer;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.Out;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public sealed class SnapshotSyncerClient
    {
        readonly Action<TickId> OnNotifyExpectingTickId;
        public sbyte clientInputTickCountAheadOfServer;
        public bool hasLastReceivedMonotonicTimeLowerBits;
        public MonotonicTimeLowerBits.MonotonicTimeLowerBits lastReceivedMonotonicTimeLowerBits;

        public SnapshotSyncerClient(EndpointId id, Action<TickId> onNotifyExpectingTickId)
        {
            OnNotifyExpectingTickId = onNotifyExpectingTickId;
            Endpoint = id;
        }

        public bool HasReceivedInitialState => RemoteIsExpectingTickId.tickId != 0;

        public TickId RemoteIsExpectingTickId { private set; get; }

        public Dictionary<LocalPlayerIndex, LocalPlayerAssignments> AssignedPredictedEntityForLocalPlayers { get; } = new();

        public uint[] AssignedPredictedEntityIdValuesForLocalPlayers => AssignedPredictedEntityForLocalPlayers.Select(x => (uint)x.Value.entityToControl.Value).ToArray();

        public bool WantsResend { get; private set; }

        public EndpointId Endpoint { get; }
        public OrderedDatagramsSequenceIdIncrease DatagramsSequenceIdIncrease { get; } = new();

        public void SetEntityToControl(LocalPlayerIndex localPlayerIndex, EntityId predictEntityId, bool shouldPredict)
        {
            if (predictEntityId.Value == 0)
            {
                throw new("entity must be set");
            }

            var wasFound = AssignedPredictedEntityForLocalPlayers.TryGetValue(localPlayerIndex, out var existingAssignment);
            if (!wasFound)
            {
                throw new Exception($"can not set an predicted entity for {localPlayerIndex} before player slot entity Id is assigned");
            }

            existingAssignment.entityToControl = predictEntityId;
            existingAssignment.shouldPredict = shouldPredict;
        }

        public void SetAssignedPlayerSlotEntity(LocalPlayerIndex localPlayerIndex, EntityId playerSlotEntity)
        {
            if (playerSlotEntity.Value == 0)
            {
                throw new("entity must be set");
            }

            var wasFound = AssignedPredictedEntityForLocalPlayers.TryGetValue(localPlayerIndex, out var existingAssignment);
            if (!wasFound)
            {
                existingAssignment = new(playerSlotEntity);
                AssignedPredictedEntityForLocalPlayers.Add(localPlayerIndex, existingAssignment);
            }
            else
            {
                existingAssignment.entityToControl = playerSlotEntity;
            }

            
        }

        public void SetExpectedTickIdByRemote(TickId tickId, uint droppedCount)
        {
            if (tickId != RemoteIsExpectingTickId)
            {
                OnNotifyExpectingTickId(tickId);
            }

            RemoteIsExpectingTickId = tickId;
            WantsResend = droppedCount > 0;
        }
    }
}