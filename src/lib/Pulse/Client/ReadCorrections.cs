/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.LogicalInput;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge.Pulse.Client
{
    public class ReadCorrection
    {
        private readonly PredictionStateChecksumQueue predictionStateChecksumHistory = new();
        private readonly RollbackQueue rollbackQueue = new();
        private EntityId assignedPredictionEntityId;

        private bool hasReceivedAssignedPredictionEntityId;

        public LogicalInputQueue PredictedInputs { get; } = new();

        public (bool, EntityId) AssignedPredictEntityId =>
            (hasReceivedAssignedPredictionEntityId, assignedPredictionEntityId);

        /// <summary>
        ///     Handles incoming correction states
        ///     If checksum is not the same, it rollbacks, replicate, and rollforth.
        /// </summary>
        /// <param name="correctionsForTickId"></param>
        /// <param name="snapshotReader"></param>
        /// <param name="world"></param>
        /// <param name="log"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ReadCorrections(TickId correctionsForTickId, IOctetReader snapshotReader, IEntityContainer world,
            ILog log)
        {
            log.DebugLowLevel("we have corrections for {TickId}, clear old predicted inputs", correctionsForTickId);

            var correctionsCount = snapshotReader.ReadUInt16();
            if (correctionsCount > 1)
            {
                throw new NotImplementedException("no supported for more than 1 corrections count");
            }

            for (var i = 0; i < correctionsCount; ++i)
            {
                PredictedInputs.DiscardUpToAndExcluding(correctionsForTickId);
                var (targetEntityId, localPlayerIndex, octetCount) = CorrectionsHeaderReader.Read(snapshotReader);
                log.DebugLowLevel("CorrectionsHeader {EntityId}, {LocalPlayerIndex} {Checksum}", targetEntityId,
                    localPlayerIndex, octetCount);

                var correctionPayload = snapshotReader.ReadOctets(octetCount);

                var incomingFnvChecksum = Fnv.Fnv.ToFnv(correctionPayload);
                var storedPredictionStateChecksum =
                    predictionStateChecksumHistory.DequeueForTickId(correctionsForTickId);
                if (storedPredictionStateChecksum.fnvChecksum == incomingFnvChecksum)
                {
                    log.DebugLowLevel("prediction for {TickId} was correct, continuing", correctionsForTickId);
                    continue;
                }

                if (!hasReceivedAssignedPredictionEntityId)
                {
                    hasReceivedAssignedPredictionEntityId = true;
                    assignedPredictionEntityId = targetEntityId;
                }
                else
                {
                    if (targetEntityId.Value != assignedPredictionEntityId.Value)
                    {
                        throw new NotImplementedException("not implemented the ability to switch entity to predict");
                    }
                }

                log.Notice("Mis-predict at {TickId} for entity {EntityId}", correctionsForTickId, targetEntityId);
                var targetEntity = world.FetchEntity(targetEntityId);

                RollBacker.Rollback(targetEntity, rollbackQueue, correctionsForTickId);
                Replicator.Replicate(targetEntity, correctionPayload);
                RollForth.Rollforth(targetEntity, PredictedInputs, rollbackQueue, predictionStateChecksumHistory);

                targetEntity.RollMode = EntityRollMode.Predict;
            }
        }

        public void Predict(TickId predictTickId, IEntityContainer world)
        {
            if (!hasReceivedAssignedPredictionEntityId)
            {
                throw new Exception("can not predict, we do not have any assigned prediction entity");
            }

            var predictEntity = world.FetchEntity(assignedPredictionEntityId);
            PredictionTicker.Predict(predictEntity, predictTickId, rollbackQueue, predictionStateChecksumHistory);
        }
    }
}