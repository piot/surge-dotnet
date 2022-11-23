/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.Pulse.Client
{
    public sealed class LocalPlayerInput
    {
        readonly ILog log;

        public LocalPlayerInput(LocalPlayerIndex localPlayerIndex, AvatarPredictor avatarPredictor, ILog log)
        {
            LocalPlayerIndex = localPlayerIndex;
            this.log = log;
            AvatarPredictor = avatarPredictor;
        }

        public AvatarPredictor AvatarPredictor { get; }

        public LocalPlayerIndex LocalPlayerIndex { get; }

        public void SwitchEntity(EntityId assignedEntity, AvatarPredictor avatarPredictor)
        {
            //AvatarPredictor = new(LocalPlayerIndex.Value, assignedEntity, log.SubLog("AvatarPredictor"));
        }

        public override string ToString()
        {
            return
                $"[LocalPlayerInput {LocalPlayerIndex} Entity:{AvatarPredictor.EntityPredictor.AssignedAvatar} predictBuffer:{AvatarPredictor.EntityPredictor.Count} ]";
        }
    }
}