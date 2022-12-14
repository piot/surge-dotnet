/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Tick;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.Pulse.Client
{
    public static class InputComponentsSerializer
    {
        public static LogicalInput.LogicalInput[] SerializeInputComponentsFromAssignedEntity(TickId predictTickId,
            IDataSender toHostDataSender,
            IEnumerable<LocalPlayerInput> localPlayerIndices, ILog log)
        {
            var logicalInputs = new List<LogicalInput.LogicalInput>();

            var componentTypeIdForInput = (ushort)DataInfo.inputComponentTypeIds![0];
            var bitWriter = new BitWriter(64);
            foreach (var localPlayerInfo in localPlayerIndices)
            {
                var assignedEntityId = localPlayerInfo.AvatarPredictor.EntityPredictor.AssignedAvatar;
                log.DebugLowLevel("Fetch input from {LocalPlayerIndex}", assignedEntityId);

                ReadOnlySpan<byte> inputOctets;
                if (toHostDataSender.HasComponentTypeId(assignedEntityId.Value, componentTypeIdForInput))
                {
                    bitWriter.Reset();
                    ComponentTypeIdWriter.Write(bitWriter, new(componentTypeIdForInput));
                    toHostDataSender.WriteFull(bitWriter, assignedEntityId.Value, componentTypeIdForInput);

                    ComponentTypeIdWriter.Write(bitWriter, ComponentTypeId.None);
                    inputOctets = bitWriter.Close(out var position);
                }
                else
                {
                    log.Notice("Could not find input from {LocalPlayerIndex}", assignedEntityId);
                    inputOctets = ReadOnlySpan<byte>.Empty;
                }

                var logicalInput = new LogicalInput.LogicalInput(localPlayerInfo.LocalPlayerIndex, predictTickId,
                    inputOctets);
                logicalInputs.Add(logicalInput);
            }

            return logicalInputs.ToArray();
        }
    }
}