/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Host
{

    public interface IEntityManagerReceiver
    {
        void ReceiveMultipleComponentsFullFiltered(IBitReader bitReader, uint entityId, uint[] dataTypeIds);
    }

    public static class SetInputFromClients
    {
        public static void SetInputsFromClientsToEntities(IEnumerable<ConnectionToClient> orderedConnections,
            TickId authoritativeTickId, IEntityManagerReceiver entityManagerReceiver, ILog log)
        {
            foreach (var connection in orderedConnections)
            {
                log.DebugLowLevel("checking inputs from connection {Connection}", connection);
                foreach (var connectionPlayer in connection.ConnectionPlayers.Values)
                {
                    var logicalInputQueue = connectionPlayer.LogicalInputQueue;
                    if (!logicalInputQueue.HasInputForTickId(authoritativeTickId))
                    {
                        // The old data on the input is intentionally kept
                        log.Notice("connection {Connection} didn't have an input for tick {AuthoritativeTickId}",
                            connection, authoritativeTickId);
                        connection.NotifyThatInputWasTooLate(authoritativeTickId);
                        continue;
                    }

                    logicalInputQueue.DiscardUpToAndExcluding(authoritativeTickId);

                    var input = logicalInputQueue.Dequeue();

                    log.DebugLowLevel("dequeued logical input {ConnectionPlayer} {Input}", connectionPlayer, input);

                    {
                        var targetEntity =
                            connectionPlayer
                                .AssignedPredictEntity;
                        if (targetEntity.Value == 0)
                        {
                            log.Notice("target entity is null, can not apply input");
                            continue;
                        }


                        var inputReader = new BitReader(input.payload.Span, input.payload.Length * 8);
                        log.DebugLowLevel("setting input for {TickId} {PlayerIndex} {Entity}", input.appliedAtTickId,
                            connectionPlayer.LocalPlayerIndex, targetEntity);

                        entityManagerReceiver.ReceiveMultipleComponentsFullFiltered(inputReader, connectionPlayer.AssignedPredictEntity.Value, DataInfo.inputComponentTypeIds!);
                    }
                }
            }
        }
    }
}