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
    public class ReplayWriter
    {
        private readonly RaffWriter raffWriter;
        private bool isInitialized;
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
            var totalWriter = new OctetWriter((uint)(completeState.Payload.Length + 10));

            WriteCompleteStateHeader(totalWriter, completeState.TickId);
            //totalWriter.WriteUInt32((ushort)completeState.Payload.Length);
            totalWriter.WriteOctets(completeState.Payload);

            raffWriter.WriteChunk(Constants.CompleteStateIcon, Constants.CompleteStateName, totalWriter.Octets);
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

            var totalWriter = new OctetWriter((uint)(deltaState.Payload.Length + 10));
            WriteDeltaHeader(totalWriter, deltaState.TickIdRange);
            totalWriter.WriteOctets(deltaState.Payload);
            raffWriter.WriteChunk(Constants.DeltaStateIcon, Constants.DeltaStateName, totalWriter.Octets);
            lastInsertedDeltaStateRange = deltaState.TickIdRange;
        }

        public void Close()
        {
            raffWriter.Close();
        }
    }
}