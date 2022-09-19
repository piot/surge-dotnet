/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Piot.Flood;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.LogicalInput.Serialization
{
    public static class LogicalInputDeserialize
    {
        /// <summary>
        ///     Deserializes game specific input arriving on the host from the client.
        ///     Currently only one (1) input stream is supported (one local player on each client).
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LogicalInputsForAllLocalPlayers Deserialize(IOctetReader reader)
        {
            var localPlayerCount = reader.ReadUInt8();
            if (localPlayerCount == 0)
            {
                return new LogicalInputsForAllLocalPlayers(Array.Empty<LogicalInputArrayForPlayer>());
            }

            var players = new List<LogicalInputArrayForPlayer>();
            for (var localPlayerIndex = 0; localPlayerIndex < localPlayerCount; ++localPlayerIndex)
            {
                var inputCount = reader.ReadUInt8();
                if (inputCount == 0)
                {
                    continue;
                }

                var array = new LogicalInput[inputCount];

                var firstFrameId = TickIdReader.Read(reader);

                for (var i = 0; i < inputCount; ++i)
                {
                    var payloadOctetCount = reader.ReadUInt8();
                    if (payloadOctetCount > 70)
                    {
                        throw new Exception("suspicious input deltaSnapshotPackPayload octet count");
                    }

                    LogicalInput input = new(new LocalPlayerIndex((byte)localPlayerIndex),
                        new TickId((uint)(firstFrameId.tickId + i)),
                        reader.ReadOctets(payloadOctetCount));

                    array[i] = input;
                }

                var play = new LogicalInputArrayForPlayer(new LocalPlayerIndex((byte)localPlayerIndex), array);
                players.Add(play);
            }

            return new LogicalInputsForAllLocalPlayers(players.ToArray());
        }
    }
}