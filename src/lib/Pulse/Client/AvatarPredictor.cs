/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public class AvatarPredictor
    {
        private readonly IEntity assignedAvatar;
        private readonly ILog log;

        private readonly PredictionStateChecksumQueue predictionStateChecksumHistory = new();
        private readonly RollbackQueue rollbackQueue = new();

        public AvatarPredictor(LocalPlayerIndex localPlayerIndex, IEntity assignedAvatar, ILog log)
        {
            this.log = log;
            LocalPlayerIndex = localPlayerIndex;
            this.assignedAvatar = assignedAvatar;
        }

        public LogicalInputQueue PredictedInputs { get; } = new();

        public LocalPlayerIndex LocalPlayerIndex { get; }

        /// <summary>
        ///     Handles incoming correction state
        ///     If checksum is not the same, it rollbacks, replicate, and rollforth.
        /// </summary>
        /// <param name="correctionsForTickId"></param>
        /// <param name="snapshotReader"></param>
        /// <param name="world"></param>
        /// <param name="log"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ReadCorrection(TickId correctionForTickId, ReadOnlySpan<byte> correctionPayload)
        {
            PredictedInputs.DiscardUpToAndExcluding(correctionForTickId);

            log.DebugLowLevel("CorrectionsHeader {EntityId}, {LocalPlayerIndex} {Checksum}", assignedAvatar.Id,
                LocalPlayerIndex, correctionPayload.Length);

            var storedPredictionStateChecksum =
                predictionStateChecksumHistory.DequeueForTickId(correctionForTickId);
            if (storedPredictionStateChecksum.IsEqual(correctionPayload))
            {
                log.DebugLowLevel("prediction for {TickId} was correct, continuing", correctionForTickId);
                return;
            }

            log.Notice("Mis-predict at {TickId} for entity {EntityId}", correctionForTickId, assignedAvatar.Id);

            RollBacker.Rollback(assignedAvatar, rollbackQueue, correctionForTickId);
            Replicator.Replicate(assignedAvatar, correctionPayload);
            RollForth.Rollforth(assignedAvatar, PredictedInputs, rollbackQueue, predictionStateChecksumHistory);

            assignedAvatar.RollMode = EntityRollMode.Predict;
        }

        public void Predict(LogicalInput.LogicalInput logicalInput)
        {
            assignedAvatar.RollMode = EntityRollMode.Predict;

            var inputDeserialize = assignedAvatar.GeneratedEntity as IInputDeserialize;
            if (inputDeserialize is null)
            {
                throw new Exception(
                    $"It is not possible to control Entity {assignedAvatar.Id}, it has no IDeserializeInput interface");
            }

            var inputReader = new OctetReader(logicalInput.payload.Span);
            inputDeserialize.SetInput(inputReader);
            PredictionTicker.Predict(assignedAvatar, logicalInput.appliedAtTickId, rollbackQueue,
                predictionStateChecksumHistory);
        }
    }
}