/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Surge.LogicalInput.Serialization;
using Piot.Surge.OrderedDatagrams;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.Tick;
using Piot.Transport;

namespace Piot.Surge.Pulse.Client
{
    public sealed class BundleAndSendOutInput
    {
        private readonly OctetWriter cachedDatagramWriter = new(Constants.MaxDatagramOctetSize);
        private readonly OrderedDatagramsSequenceIdIncrease datagramsOut = new();
        private readonly ILog log;
        private readonly ITransportClient transportClient;

        private TickId lastSeenSnapshotTickId;
        private TickId nextExpectedSnapshotTickId;

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

        public void BundleAndSendInputDatagram(LocalPlayerInput[] localPlayerInputs, Milliseconds now)
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

            log.Debug("Sending inputs to host {FirstTickId} {LastTickId}",
                logicalInputForAllPlayers.debugFirstId, logicalInputForAllPlayers.debugLastId);

            transportClient.SendToHost(cachedDatagramWriter.Octets);

            datagramsOut.Increase();
        }
    }
}