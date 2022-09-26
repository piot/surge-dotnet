/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.Entities;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.Pulse.Client
{
    public sealed class LocalPlayerInput
    {
        readonly ILog log;

        public LocalPlayerInput(LocalPlayerIndex localPlayerIndex, IEntity assignedEntity, ILog log)
        {
            LocalPlayerIndex = localPlayerIndex;
            AssignedEntity = assignedEntity;
            this.log = log;
            AvatarPredictor = new(localPlayerIndex.Value, assignedEntity, log.SubLog("AvatarPredictor"));
        }

        public AvatarPredictor AvatarPredictor { get; private set; }

        public IEntity AssignedEntity { get; }

        public LocalPlayerIndex LocalPlayerIndex { get; }

        public void SwitchEntity(IEntity assignedEntity)
        {
            AvatarPredictor = new(LocalPlayerIndex.Value, assignedEntity, log.SubLog("AvatarPredictor"));
        }
    }
}