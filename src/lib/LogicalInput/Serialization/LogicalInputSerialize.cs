/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Runtime.CompilerServices;
using Piot.Flood;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.LogicalInput.Serialization
{
    public static class LogicalInputSerialize
    {
        /// <summary>
        ///     Serializing the game specific inputs to be sent from the client to the authoritative host.
        ///     The inputs should be fed to this method with redundancy. All outstanding inputs should be
        ///     sent each network tick in order to handle packet drops.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Serialize(IOctetWriter writer, LogicalInputsForAllLocalPlayers inputsForLocalPlayers)
        {
            writer.WriteUInt8((byte)inputsForLocalPlayers.inputForEachPlayerInSequence.Length);

            foreach (var inputsForPlayer in inputsForLocalPlayers.inputForEachPlayerInSequence)
            {
                var tickCount = inputsForPlayer.inputs.Length;
                if (tickCount > 255)
                {
                    throw new("too many inputs to serialize");
                }

                writer.WriteUInt8((byte)tickCount);
                if (tickCount == 0)
                {
                    continue;
                }

                var first = inputsForPlayer.inputs[0];
                TickIdWriter.Write(writer, first.appliedAtTickId);

                var expectedTickIdValue = first.appliedAtTickId.tickId;
                foreach (var logicalInput in inputsForPlayer.inputs)
                {
                    if (logicalInput.appliedAtTickId.tickId != expectedTickIdValue)
                    {
                        throw new(
                            $"logical input in wrong order in collection. Expected {expectedTickIdValue} but received {logicalInput.appliedAtTickId.tickId}");
                    }

                    writer.WriteUInt8((byte)logicalInput.payload.Length);
                    writer.WriteOctets(logicalInput.payload.Span);

                    expectedTickIdValue++;
                }
            }
        }
    }
}