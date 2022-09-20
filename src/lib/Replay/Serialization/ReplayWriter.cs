/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Raff.Stream;
using Piot.SerializableVersion.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class ReplayWriter
    {
        readonly OctetWriter cachedStateWriter = new(16 * 1024);
        readonly uint framesBetweenCompleteState;
        readonly RaffWriter raffWriter;
        readonly ReplayFileSerializationInfo info;

        uint packCountSinceCompleteState;

        public ReplayWriter(CompleteState completeState, ReplayVersionInfo replayVersionInfo,
            ReplayFileSerializationInfo info, IOctetWriter writer,
            uint framesUntilCompleteState = 60)
        {
            this.info = info;
            framesBetweenCompleteState = framesUntilCompleteState;
            raffWriter = new(writer);
            WriteVersionChunk(replayVersionInfo);
            packCountSinceCompleteState = 60;
            AddCompleteState(completeState);
        }

        public bool NeedsCompleteState => framesBetweenCompleteState != 0 &&
                                          packCountSinceCompleteState >= framesBetweenCompleteState;

        bool AllowedToAddCompleteState => framesBetweenCompleteState == 0 || NeedsCompleteState;

        void WriteVersionChunk(ReplayVersionInfo replayVersionInfo)
        {
            var writer = new OctetWriter(100);
            VersionWriter.Write(writer, replayVersionInfo.applicationSemanticVersion);
            VersionWriter.Write(writer, replayVersionInfo.surgeProtocolSemanticVersion);
            raffWriter.WriteChunk(info.FileInfo.Icon, info.FileInfo.Name, writer.Octets);
        }

        static void WriteCompleteStateHeader(IOctetWriter writer, TimeMs timeNowMs, TickId tickId)
        {
            writer.WriteUInt8(0x02);
            writer.WriteUInt64((ulong)timeNowMs.ms);
            TickIdWriter.Write(writer, tickId);
        }

        public void AddCompleteState(CompleteState completeState)
        {
            if (!AllowedToAddCompleteState)
            {
                throw new("Not allowed to insert complete state now");
            }


            packCountSinceCompleteState = 0;

            cachedStateWriter.Reset();
            WriteCompleteStateHeader(cachedStateWriter, completeState.CapturedAtTimeMs, completeState.TickId);
            //totalWriter.WriteUInt32((ushort)completeState.Payload.Length);
            cachedStateWriter.WriteOctets(completeState.Payload);

            raffWriter.WriteChunk(info.CompleteStateInfo.Icon, info.CompleteStateInfo.Name, cachedStateWriter.Octets);
        }

        static void WriteDeltaHeader(IOctetWriter writer, TimeMs timeNowMs, TickIdRange tickIdRange)
        {
            writer.WriteUInt8(0x01);
            var lowerBits = MonotonicTimeLowerBits.MonotonicTimeLowerBits.FromTime(timeNowMs);
            MonotonicTimeLowerBitsWriter.Write(lowerBits, writer);
            TickIdRangeWriter.Write(writer, tickIdRange);
        }

        public void AddDeltaState(DeltaState deltaState)
        {
            if (NeedsCompleteState)
            {
                throw new($"needs complete state now, been {packCountSinceCompleteState} since last one");
            }

            cachedStateWriter.Reset();
            WriteDeltaHeader(cachedStateWriter, deltaState.TimeProcessedMs, deltaState.TickIdRange);
            cachedStateWriter.WriteOctets(deltaState.Payload);
            raffWriter.WriteChunk(info.DeltaStateInfo.Icon, info.DeltaStateInfo.Name, cachedStateWriter.Octets);
        }

        public void Close()
        {
            raffWriter.Close();
        }
    }
}