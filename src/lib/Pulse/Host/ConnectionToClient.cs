/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.DatagramType.Serialization;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol.ReceiveStatus;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Host
{
    public sealed class ConnectionToClient
    {
        readonly ILog log;
        readonly OrderedDatagramsInChecker orderedDatagramsIn = new();
        readonly SnapshotSyncerClient syncer;

        public ConnectionToClient(EndpointId id, SnapshotSyncerClient syncer, ILog log)
        {
            Id = id;
            this.log = log;
            this.syncer = syncer;
        }

        public Dictionary<uint, ConnectionPlayer> ConnectionPlayers { get; } = new();

        public EndpointId Id { get; }

        public override string ToString()
        {
            return $"[ConnectionToClient {Id} playerCount:{ConnectionPlayers.Count}]";
        }

        public void NotifyThatInputWasTooLate(TickId authoritativeTickId)
        {
            // TODO: Send information to client about that input was too late, and is guessed by host
        }

        public void AssignPredictEntityToPlayer(LocalPlayerIndex localPlayerIndex, IEntity entity)
        {
            ConnectionPlayer connectionPlayer;

            if (!ConnectionPlayers.ContainsKey(localPlayerIndex.Value))
            {
                connectionPlayer = new(Id, localPlayerIndex);
                ConnectionPlayers[localPlayerIndex.Value] = connectionPlayer;
            }
            else
            {
                connectionPlayer = ConnectionPlayers[localPlayerIndex.Value];
            }

            connectionPlayer.AssignedPredictEntity = entity;
            syncer.SetAssignedPredictedEntity(localPlayerIndex, entity);
        }

        void ReceivePredictedInputs(IOctetReader reader, TickId serverIsAtTickId)
        {
            syncer.lastReceivedMonotonicTimeLowerBits = MonotonicTimeLowerBitsReader.Read(reader);
            syncer.hasLastReceivedMonotonicTimeLowerBits = true;

            log.Debug("received predicted inputs {LowerBits}", syncer.lastReceivedMonotonicTimeLowerBits);
            SnapshotReceiveStatusReader.Read(reader, out var expectingTickId, out var droppedFrames);

            syncer.SetExpectedTickIdByRemote(expectingTickId, droppedFrames);

            var logicalInputs = LogicalInputDeserialize.Deserialize(reader);


            log.Debug("received predicted input count {Count}", logicalInputs.inputForEachPlayerInSequence.Length);
            foreach (var logicalInputArrayForPlayer in logicalInputs.inputForEachPlayerInSequence)
            {
                ConnectionPlayers.TryGetValue(logicalInputArrayForPlayer.localPlayerIndex.Value,
                    out var connectionPlayer);
                if (connectionPlayer is null)
                {
                    log.Notice(
                        "got input for a connection player that isn't created yet. creating a new one {PlayerIndex}",
                        logicalInputArrayForPlayer.localPlayerIndex);
                    connectionPlayer = new(Id, logicalInputArrayForPlayer.localPlayerIndex);
                }

                var logicalInputQueue = connectionPlayer.LogicalInputQueue;

                var first = logicalInputArrayForPlayer.inputs[0];

                if ((first.appliedAtTickId > logicalInputQueue.WaitingForTickId &&
                     logicalInputQueue.IsInitialized) || (serverIsAtTickId > logicalInputQueue.WaitingForTickId &&
                                                          logicalInputQueue.IsInitialized))
                {
                    log.Notice(
                        $"there is a gap in the input queue. Input queue is waiting for {logicalInputQueue.WaitingForTickId} but first received in this datagram {first.appliedAtTickId}. Reset it so we can receive the inputs");
                    logicalInputQueue.Reset();
                }

                foreach (var logicalInput in logicalInputArrayForPlayer.inputs)
                {
                    if (logicalInput.appliedAtTickId.tickId < serverIsAtTickId.tickId)
                    {
                        // log.Notice("Host has passed this tickId already, skipping input {TickId} {HostTickId}",
                        //   logicalInput.appliedAtTickId, serverIsAtTickId);
                        continue;
                    }

                    if (logicalInput.appliedAtTickId.tickId > logicalInputQueue.WaitingForTickId.tickId)
                    {
                        log.Notice("Input is in future of the queue, must reset queue {InputTickId} {QueueTickId}",
                            logicalInput.appliedAtTickId, logicalInputQueue.WaitingForTickId.tickId);
                        logicalInputQueue.Reset();
                    }

                    logicalInputQueue.AddLogicalInput(logicalInput);
                    log.DebugLowLevel("added input for {LocalPlayerIndex} tickId {TickId}",
                        logicalInput.localPlayerIndex, logicalInput.appliedAtTickId);
                }

                log.DebugLowLevel("input Queue on server for connection {ConnectionId} is {Count}", Id,
                    ConnectionPlayers.Count);
            }

            syncer.clientInputTickCountAheadOfServer = (sbyte)ConnectionPlayers.Count;
        }

        public void Receive(IOctetReader reader, TickId serverIsAtTickId)
        {
            var shouldBeReceived = orderedDatagramsIn.ReadAndCheck(reader);
            if (!shouldBeReceived)
            {
                log.Notice("skipping datagram {OrderedDatagramsSequenceId} {EncounteredSequenceId}", orderedDatagramsIn.LastValue, orderedDatagramsIn.DebugLastReadValue);
                return;
            }

            var datagramType = DatagramTypeReader.Read(reader);
            switch (datagramType)
            {
                case DatagramType.DatagramType.PredictedInputs:
                    ReceivePredictedInputs(reader, serverIsAtTickId);
                    break;
                default:
                    throw new DeserializeException($"illegal datagram type {datagramType} from client ${Id}");
            }
        }
    }
}