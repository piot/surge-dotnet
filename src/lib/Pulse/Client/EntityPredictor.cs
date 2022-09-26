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
        readonly ILog log;

        public EntityPredictor(IEntity assignedAvatar, ILog log)
        {
            AssignedAvatar = assignedAvatar;
            this.log = log;
        }

        public LogicalInputQueue PredictedInputs { get; } = new();
        public IEntity AssignedAvatar { get; }

        public RollbackStack RollbackStack { get; } = new();

        public PredictionStateChecksumQueue PredictionStateHistory { get; } = new();

        public OctetWriter UndoWriter { get; } = new(1024);

        public TickId TickId => PredictionStateHistory.LastTickId;
        public TickId FirstTickId => PredictionStateHistory.FirstTickId;
        public TickId EndTickId => PredictedInputs.Last.appliedAtTickId;

        public int Count => PredictedInputs.Count < PredictionStateHistory.Count
            ? PredictedInputs.Count
            : PredictionStateHistory.Count;

        public void AssertInternalState()
        {
            if (PredictedInputs.Count == 0)
            {
                if (PredictionStateHistory.Count != 0)
                {
                    throw new($"state history and predicted inputs count misaligned {PredictionStateHistory.Count}");
                }

                return;
            }

            if (RollbackStack.EndTickId() != PredictionStateHistory.LastTickId ||
                RollbackStack.PeekTickId() != PredictedInputs.Peek().appliedAtTickId)
            {
                log.Debug("state history and stack are misaligned {EndTickId} {HistoryTickId}",
                    RollbackStack.EndTickId(), PredictionStateHistory.LastTickId);
                throw new InvalidOperationException(
                    $"state history and stack are misaligned {RollbackStack.EndTickId()} {PredictionStateHistory.LastTickId}");
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

            AssignedAvatar.CompleteEntity.RollMode = EntityRollMode.Predict;

            if (AssignedAvatar.CompleteEntity is not IInputDeserialize inputDeserialize)
            {
                throw new(
                    $"It is not possible to control Entity {AssignedAvatar.Id}, it has no IDeserializeInput interface");
            }

            var inputReader = new OctetReader(logicalInput.payload.Span);
            inputDeserialize.SetInput(inputReader);
            UndoWriter.Reset();
            log.Info("Predict and save for {TickId}", logicalInput.appliedAtTickId);
            PredictionTickAndStateSave.PredictAndStateSave(AssignedAvatar, logicalInput.appliedAtTickId, RollbackStack,
                PredictionStateHistory, PredictMode.Predicting, UndoWriter);
        }
    }
}