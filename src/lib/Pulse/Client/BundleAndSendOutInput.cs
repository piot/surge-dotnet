/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.LogicalInput.Serialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public sealed class BundleAndSendOutInput
    {
        readonly OctetWriter cachedDatagramWriter = new(Constants.MaxDatagramOctetSize);
        readonly OrderedDatagramsSequenceIdIncrease datagramsOut = new();
        readonly ILog log;
        readonly ITransportClient transportClient;

        TickId lastSeenSnapshotTickId;
        TickId nextExpectedSnapshotTickId;

        public BundleAndSendOutInput(ITransportClient transportClient, ILog log)
        {
            this.transportClient = transportClient;
            this.log = log;
        }

        public TickId LastSeenSnapshotTickId
        {
            set => lastSeenSnapshotTickId = value;
        }

        public TickId NextExpectedSnapshotTickId
        {
            set => nextExpectedSnapshotTickId = value;
        }

        public void BundleAndSendInputDatagram(LocalPlayerInput[] localPlayerInputs, TimeMs now)
        {
            var logicalInputForAllPlayers =
                LocalPlayerLogicalInputBundler.BundleInputForAllLocalPlayers(localPlayerInputs);

            var droppedSnapshotCount = lastSeenSnapshotTickId > nextExpectedSnapshotTickId
                ? (byte)(lastSeenSnapshotTickId - nextExpectedSnapshotTickId).tickId
                : (byte)0;
            cachedDatagramWriter.Reset();

            LogicInputDatagramSerialize.Serialize(cachedDatagramWriter, datagramsOut.Value, nextExpectedSnapshotTickId,
                droppedSnapshotCount,
                now, logicalInputForAllPlayers);

            log.DebugLowLevel(
                "Sending inputs to host {FirstInputTickId} {HighestInputTickId} {NextExpectedSnapshotTickId} {DroppedCount}",
                logicalInputForAllPlayers.debugFirstId, logicalInputForAllPlayers.debugLastId,
                nextExpectedSnapshotTickId, droppedSnapshotCount);

            transportClient.SendToHost(cachedDatagramWriter.Octets);

            datagramsOut.Increase();
        }
    }
}