/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;
using Piot.Surge.LogicalInput;

namespace Piot.Surge.Pulse.Client
{
    public sealed class LocalPlayerInput
    {
        public LocalPlayerInput(LocalPlayerIndex localPlayerIndex, IEntity assignedEntity)
        {
            LocalPlayerIndex = localPlayerIndex;
            AssignedEntity = assignedEntity;
        }

        public IEntity AssignedEntity { get; }

        public LogicalInputQueue PredictedInputs { get; } = new();

        public LocalPlayerIndex LocalPlayerIndex { get; }
    }
}