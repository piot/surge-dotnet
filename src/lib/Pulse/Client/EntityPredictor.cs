/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.LogicalInput;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public class EntityPredictor
    {
        readonly IEntity assignedAvatar;
        readonly ILog log;
        readonly PredictionStateChecksumQueue predictionStateHistory = new();
        readonly RollbackStack rollbackStack = new();
        readonly OctetWriter cachedUndoWriter = new(1024);

        public LogicalInputQueue PredictedInputs { get; } = new();
        public IEntity AssignedAvatar => assignedAvatar;
        public RollbackStack RollbackStack => rollbackStack;
        public PredictionStateChecksumQueue PredictionStateHistory => predictionStateHistory;

        public OctetWriter UndoWriter => cachedUndoWriter;

        public EntityPredictor(IEntity assignedAvatar, ILog log)
        {
            this.assignedAvatar = assignedAvatar;
            this.log = log;
        }

        public TickId TickId => predictionStateHistory.LastTickId;
        public TickId FirstTickId => predictionStateHistory.FirstTickId;
        public TickId EndTickId => PredictedInputs.Last.appliedAtTickId;

        public void AssertInternalState()
        {
            if (PredictedInputs.Count == 0)
            {
                return;
            }
            
            if (rollbackStack.EndTickId() != predictionStateHistory.LastTickId || rollbackStack.PeekTickId() != PredictedInputs.Peek().appliedAtTickId)
            {
                log.Debug("state history and stack are misaligned {EndTickId} {HistoryTickId}", rollbackStack.EndTickId(), predictionStateHistory.LastTickId);
                throw new InvalidOperationException(
                    $"state history and stack are misaligned {rollbackStack.EndTickId()} {predictionStateHistory.LastTickId}");
            }
        }

        public void DiscardUpToAndExcluding(TickId correctionForTickId)
        {
            PredictedInputs.DiscardUpToAndExcluding(correctionForTickId);
            RollbackStack.DiscardUpToAndExcluding(correctionForTickId);
        }



        public void Predict(LogicalInput.LogicalInput logicalInput)
        {
            AssertInternalState();
            
            assignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;

            if (assignedAvatar.CompleteEntity is not IInputDeserialize inputDeserialize)
            {
                throw new(
                    $"It is not possible to control Entity {assignedAvatar.Id}, it has no IDeserializeInput interface");
            }

            var inputReader = new OctetReader(logicalInput.payload.Span);
            inputDeserialize.SetInput(inputReader);
            cachedUndoWriter.Reset();
            log.Info("Predict and save for {TickId}", logicalInput.appliedAtTickId);
            PredictionTickAndStateSave.PredictAndStateSave(assignedAvatar, logicalInput.appliedAtTickId, RollbackStack,
                PredictionStateHistory, PredictMode.Predicting, cachedUndoWriter);
        }
    }
}