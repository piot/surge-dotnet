/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Raff.Stream;
using Piot.SerializableVersion.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class ReplayWriter
    {
        private readonly RaffWriter raffWriter;
        private readonly OctetWriter cachedStateWriter = new(16 * 1024);
        private TickIdRange lastInsertedDeltaStateRange;
        private uint packCountSinceCompleteState;

        public ReplayWriter(CompleteState completeState, ReplayVersionInfo replayVersionInfo, IOctetWriter writer)
        {
            raffWriter = new RaffWriter(writer);
            WriteVersionChunk(replayVersionInfo);
            packCountSinceCompleteState = 60;
            lastInsertedDeltaStateRange = TickIdRange.FromTickId(completeState.TickId);
            AddCompleteState(completeState);
        }

        public bool NeedsCompleteState => packCountSinceCompleteState >= 60;

        private void WriteVersionChunk(ReplayVersionInfo replayVersionInfo)
        {
            var writer = new OctetWriter(100);
            VersionWriter.Write(writer, replayVersionInfo.applicationSemanticVersion);
            VersionWriter.Write(writer, replayVersionInfo.surgeProtocolSemanticVersion);
            raffWriter.WriteChunk(Constants.ReplayIcon, Constants.ReplayName, writer.Octets);
        }

        private static void WriteCompleteStateHeader(IOctetWriter writer, TickId tickId)
        {
            writer.WriteUInt8(0x02);
            TickIdWriter.Write(writer, tickId);
        }

        public void AddCompleteState(CompleteState completeState)
        {
            if (!NeedsCompleteState)
            {
                throw new Exception("Not allowed to insert complete state now");
            }

            if (completeState.TickId != lastInsertedDeltaStateRange.Last)
            {
                throw new ArgumentOutOfRangeException(nameof(completeState),
                    $"complete state must be {lastInsertedDeltaStateRange.Last}, but encountered {completeState.TickId}");
            }

            packCountSinceCompleteState = 0;

            cachedStateWriter.Reset();
            WriteCompleteStateHeader(cachedStateWriter, completeState.TickId);
            //totalWriter.WriteUInt32((ushort)completeState.Payload.Length);
            cachedStateWriter.WriteOctets(completeState.Payload);

            raffWriter.WriteChunk(Constants.CompleteStateIcon, Constants.CompleteStateName, cachedStateWriter.Octets);
        }

        private static void WriteDeltaHeader(IOctetWriter writer, TickIdRange tickIdRange)
        {
            writer.WriteUInt8(0x01);
            TickIdRangeWriter.Write(writer, tickIdRange);
        }

        public void AddDeltaState(DeltaState deltaState)
        {
            if (NeedsCompleteState)
            {
                throw new Exception($"needs complete state now, been {packCountSinceCompleteState} since last one");
            }

            if (!lastInsertedDeltaStateRange.CanAppend(deltaState.TickIdRange))
            {
                throw new Exception($"not appendable {lastInsertedDeltaStateRange} and {deltaState.TickIdRange}");
            }

            cachedStateWriter.Reset();
            WriteDeltaHeader(cachedStateWriter, deltaState.TickIdRange);
            cachedStateWriter.WriteOctets(deltaState.Payload);
            raffWriter.WriteChunk(Constants.DeltaStateIcon, Constants.DeltaStateName, cachedStateWriter.Octets);
            lastInsertedDeltaStateRange = deltaState.TickIdRange;
        }

        public void Close()
        {
            raffWriter.Close();
        }
    }
}