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
        private readonly Dictionary<byte, AvatarPredictor> localAvatarPredictors = new();
        private readonly ILog log;

        public ClientPredictor(ILog log)
        {
            this.log = log;
        }

        public AvatarPredictor CreateAvatarPredictor(LocalPlayerIndex localPlayerIndex, IEntity assignedEntity)
        {
            var avatarPredictor = new AvatarPredictor(localPlayerIndex, assignedEntity, log);
            localAvatarPredictors[localPlayerIndex.Value] = avatarPredictor;

            return avatarPredictor;
        }

        public void Predict(LocalPlayerInput[] localPlayerInputs)
        {
            foreach (var localPlayerInput in localPlayerInputs)
            {
                var localAvatarPredictor = localAvatarPredictors[localPlayerInput.LocalPlayerIndex.Value];
                localAvatarPredictor.Predict(localPlayerInput.PredictedInputs.Last);
            }
        }

        public void ReadCorrection(LocalPlayerIndex localPlayerIndex, TickId tickId, ReadOnlySpan<byte> payload)
        {
            var localAvatarPredictor = localAvatarPredictors[localPlayerIndex.Value];
            localAvatarPredictor.ReadCorrection(tickId, payload);
        }
    }
}