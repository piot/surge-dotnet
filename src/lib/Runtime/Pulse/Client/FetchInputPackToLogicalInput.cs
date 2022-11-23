/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class FetchInputPackToLogicalInput
    {
        public static LogicalInput.LogicalInput[] FetchLogicalInputs(TickId predictTickId,
            IDataSender componentsWriter,
            IEnumerable<LocalPlayerInput> localPlayerIndices, ILog log)
        {
            var logicalInputs = new List<LogicalInput.LogicalInput>();
            foreach (var localPlayerInfo in localPlayerIndices)
            {
                var assignedEntityId = localPlayerInfo.AvatarPredictor.EntityPredictor.AssignedAvatar;
                log.DebugLowLevel("Fetch input from {LocalPlayerIndex}", assignedEntityId);
                var bitWriter = new BitWriter(64);

                componentsWriter.WriteFull(bitWriter, assignedEntityId.Value, (ushort)DataInfo.inputComponentTypeIds![0]);

                var inputOctets = bitWriter.Close(out var position);
                var logicalInput = new LogicalInput.LogicalInput(localPlayerInfo.LocalPlayerIndex, predictTickId,
                    inputOctets);
                logicalInputs.Add(logicalInput);
            }

            return logicalInputs.ToArray();
        }
    }
}