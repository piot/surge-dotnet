/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public sealed class ClientPredictor
    {
        readonly Dictionary<byte, AvatarPredictor> localAvatarPredictors = new();
        readonly ILog log;

        public ClientPredictor(ILog log)
        {
            this.log = log;
        }

        public AvatarPredictor CreateAvatarPredictor(LocalPlayerIndex localPlayerIndex, IEntity assignedEntity)
        {
            var avatarPredictor = new AvatarPredictor(localPlayerIndex.Value, assignedEntity, log);
            localAvatarPredictors[localPlayerIndex.Value] = avatarPredictor;

            return avatarPredictor;
        }

        public EntityPredictor? FindPredictorFor(IEntity completeEntity)
        {
            foreach (var localPredictor in localAvatarPredictors.Values)
            {
                if (localPredictor.EntityPredictor.AssignedAvatar.Id.Value == completeEntity.Id.Value)
                {
                    return localPredictor.EntityPredictor;
                }
            }

            return null;
        }

        public void Predict(LocalPlayerInput[] localPlayerInputs, LogicalInput.LogicalInput[] inputThisFrame,
            bool doActualPrediction)
        {
            var index = 0;
            foreach (var localPlayerInput in localPlayerInputs)
            {
                var localAvatarPredictor = localAvatarPredictors[localPlayerInput.LocalPlayerIndex.Value];
                localAvatarPredictor.EntityPredictor.AddInput(inputThisFrame[index], doActualPrediction);
                index++;
            }
        }

        public void ReadCorrection(LocalPlayerIndex localPlayerIndex, TickId tickId, ReadOnlySpan<byte> payload)
        {
            var localAvatarPredictor = localAvatarPredictors[localPlayerIndex.Value];
            localAvatarPredictor.ReadCorrection(tickId, payload);
        }
    }
}