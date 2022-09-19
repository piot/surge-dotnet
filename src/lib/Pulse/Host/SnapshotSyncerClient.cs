/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public sealed class SnapshotSyncerClient
    {
        private readonly Action<TickId> OnNotifyExpectingTickId;
        public sbyte clientInputTickCountAheadOfServer;
        public MonotonicTimeLowerBits.MonotonicTimeLowerBits lastReceivedMonotonicTimeLowerBits;

        public SnapshotSyncerClient(EndpointId id, Action<TickId> onNotifyExpectingTickId)
        {
            OnNotifyExpectingTickId = onNotifyExpectingTickId;
            Endpoint = id;
        }

        public bool HasReceivedInitialState => RemoteIsExpectingTickId.tickId != 0;

        public TickId RemoteIsExpectingTickId { private set; get; }

        public Dictionary<uint, IEntity> AssignedPredictedEntityForLocalPlayers { get; } = new();

        public bool WantsResend { get; private set; }

        public EndpointId Endpoint { get; }
        public OrderedDatagramsSequenceIdIncrease DatagramsSequenceIdIncrease { get; } = new();

        public void SetAssignedPredictedEntity(LocalPlayerIndex localPlayerIndex, IEntity entity)
        {
            AssignedPredictedEntityForLocalPlayers[localPlayerIndex.Value] = entity;
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