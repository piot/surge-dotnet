/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Host
{
    public static class SetInputFromClients
    {
        public static void SetInputsFromClientsToEntities(IEnumerable<ConnectionToClient> orderedConnections,
            TickId serverTickId, ILog log)
        {
            foreach (var connection in orderedConnections)
            {
                log.DebugLowLevel("checking inputs from connection {Connection}", connection);
                foreach (var connectionPlayer in connection.ConnectionPlayers.Values)
                {
                    var logicalInputQueue = connectionPlayer.LogicalInputQueue;
                    if (!logicalInputQueue.HasInputForTickId(serverTickId))
                    {
                        // The old data on the input is intentionally kept
                        log.Notice($"connection {connection.Id} didn't have an input for tick {serverTickId}");
                        continue;
                    }

                    var input = logicalInputQueue.Dequeue();
                    log.DebugLowLevel("dequeued logical input {ConnectionPlayer} {Input}", connectionPlayer, input);

                    {
                        var targetEntity =
                            connectionPlayer
                                .AssignedPredictEntity;
                        if (targetEntity is null)
                        {
                            log.Notice("target entity is null, can not apply input");
                            continue;
                        }

                        if (targetEntity.CompleteEntity is not IInputDeserialize inputDeserialize)
                        {
                            throw new Exception(
                                $"It is not possible to control Entity {targetEntity}, it has no IDeserializeInput interface");
                        }

                        var inputReader = new OctetReader(input.payload.Span);
                        log.DebugLowLevel("setting input for {Entity}", targetEntity);
                        inputDeserialize.SetInput(inputReader);
                    }
                }
            }
        }
    }
}