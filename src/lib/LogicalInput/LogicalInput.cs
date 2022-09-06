/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.LocalPlayer;
using Piot.Surge.Tick;

namespace Piot.Surge.LogicalInput
{
    public static class CompareOctets
    {
        public static bool Compare(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        {
            return a.SequenceEqual(b);
        }
    }


    public readonly struct LogicalInputsForAllLocalPlayers
    {
        public readonly LogicalInputArrayForPlayer[] inputForEachPlayerInSequence;

        public LogicalInputsForAllLocalPlayers(LogicalInputArrayForPlayer[] inputForEachPlayerInSequence)
        {
            this.inputForEachPlayerInSequence = inputForEachPlayerInSequence;
        }
    }

    public struct LogicalInputArrayForPlayer
    {
        public LogicalInput[] inputs;
        public LocalPlayerIndex localPlayerIndex;

        public LogicalInputArrayForPlayer(LocalPlayerIndex localPlayerIndex,
            LogicalInput[] inputs)
        {
            this.localPlayerIndex = localPlayerIndex;
            this.inputs = inputs;
        }
    }


    /// <summary>
    ///     Serialized Game specific input stored in the <see cref="LogicalInput.payload" />.
    /// </summary>
    public struct LogicalInput
    {
        public LocalPlayerIndex localPlayerIndex;
        public TickId appliedAtTickId;
        public ReadOnlyMemory<byte> payload;

        public LogicalInput(LocalPlayerIndex localPlayerIndex, TickId appliedAtTickId, ReadOnlySpan<byte> payload)
        {
            this.localPlayerIndex = localPlayerIndex;
            this.appliedAtTickId = appliedAtTickId;
            this.payload = payload.ToArray();
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            var other = (LogicalInput)obj;

            return other.appliedAtTickId.tickId == appliedAtTickId.tickId &&
                   CompareOctets.Compare(other.payload.Span, payload.Span);
        }

        public readonly override string ToString()
        {
            return
                $"[LogicalInput TickId:{appliedAtTickId} octetSize:{payload.Length} localPlayerIndex:{localPlayerIndex}]";
        }
    }
}