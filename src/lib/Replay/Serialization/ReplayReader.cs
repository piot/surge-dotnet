/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.MonotonicTime;
using Piot.Raff.Stream;
using Piot.SerializableVersion;
using Piot.SerializableVersion.Serialization;
using Piot.Surge.MonotonicTimeLowerBits;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Replay.Serialization
{
    public sealed class ReplayReader
    {
        readonly CompleteStateEntry[] completeStateEntries;
        readonly RaffReader raffReader;
        readonly IOctetReaderWithSeekAndSkip readerWithSeek;
        readonly ReplayFileSerializationInfo info;
        TimeMs lastReadTimeMs;
        TimeMs lastTimeMsFromDeltaState;

        public ReplayReader(SemanticVersion expectedApplicationVersion, ReplayFileSerializationInfo info,
            IOctetReaderWithSeekAndSkip readerWithSeek)
        {
            this.info = info;
            this.readerWithSeek = readerWithSeek;
            raffReader = new(readerWithSeek);
            ReadVersionInfo();

            if (!expectedApplicationVersion.IsEqualDisregardSuffix(ApplicationVersion))
            {
                throw new(
                    $"version mismatch, can not use this replay file {ApplicationVersion} vs expected {expectedApplicationVersion}");
            }

            var positionBefore = readerWithSeek.Position;

            completeStateEntries =
                CompleteStateScanner.ScanForAllCompleteStatePositions(raffReader, readerWithSeek,
                    info.CompleteStateInfo);

            readerWithSeek.Seek(positionBefore);
        }

        public SemanticVersion ApplicationVersion { get; private set; }

        public SemanticVersion StateSerializationVersion { get; private set; }

        public TickId FirstCompleteStateTickId => new(completeStateEntries[0].tickId);

        void ReadVersionInfo()
        {
            var versionPack = raffReader.ReadExpectedChunk(info.FileInfo.Icon, info.FileInfo.Name);
            var reader = new OctetReader(versionPack);
            ApplicationVersion = VersionReader.Read(reader);
            StateSerializationVersion = VersionReader.Read(reader);
        }


        CompleteStateEntry FindClosestEntry(TickId tickId)
        {
            var tickIdValue = tickId.tickId;
            if (completeStateEntries.Length == 0)
            {
                throw new("unexpected that no complete states are found");
            }

            var left = 0;
            var right = completeStateEntries.Length - 1;

            while (left != right)
            {
                var middle = left + right / 2;
                var middleEntry = completeStateEntries[middle];
                if (tickIdValue == middleEntry.tickId)
                {
                    return middleEntry;
                }

                if (tickIdValue < middleEntry.tickId)
                {
                    right = middle;
                }
                else
                {
                    left = middle;
                }
            }

            var closest = completeStateEntries[left];
            if (closest.tickId <= tickIdValue)
            {
                return closest;
            }

            if (left >= 1)
            {
                var previous = completeStateEntries[left - 1];
                if (previous.tickId > tickIdValue)
                {
                    throw new("strange state in replay");
                }

                return previous;
            }

            return closest;
        }

        public CompleteState Seek(TickId closestToTick)
        {
            var findClosestEntry = FindClosestEntry(closestToTick);
            readerWithSeek.Seek(findClosestEntry.streamPosition);

            return ReadCompleteState();
        }

        public DeltaState? ReadDeltaState()
        {
            var octetLength = raffReader.ReadChunkHeader(out var icon, out var name);
            if (icon.Value == 0 && name.Value == 0)
            {
                return null;
            }

            if (icon.Value == info.CompleteStateInfo.Icon.Value)
            {
                readerWithSeek.Seek(readerWithSeek.Position + octetLength);
                return ReadDeltaState();
            }

            var beforePosition = readerWithSeek.Position;

            var type = readerWithSeek.ReadUInt8();
            if (type != 01)
            {
                throw new($"desync {type}");
            }

            var timeLowerBits = MonotonicTimeLowerBitsReader.Read(readerWithSeek);
            lastTimeMsFromDeltaState =
                LowerBitsToMonotonic.LowerBitsToPastMonotonicMs(lastTimeMsFromDeltaState, timeLowerBits);
            var tickIdRange = TickIdRangeReader.Read(readerWithSeek);

            var afterPosition = readerWithSeek.Position;
            var headerOctetCount = (int)(afterPosition - beforePosition);

            return new(lastTimeMsFromDeltaState, tickIdRange,
                readerWithSeek.ReadOctets((int)octetLength - headerOctetCount));
        }

        CompleteState ReadCompleteState()
        {
            var octetLength =
                raffReader.ReadExpectedChunkHeader(info.CompleteStateInfo.Icon, info.CompleteStateInfo.Name);
            var beforePosition = readerWithSeek.Position;

            var type = readerWithSeek.ReadUInt8();
            if (type != 02)
            {
                throw new("desync");
            }

            var time = new TimeMs((long)readerWithSeek.ReadUInt64());
            lastReadTimeMs = time;
            lastTimeMsFromDeltaState = time;
            var tickId = TickIdReader.Read(readerWithSeek);

            var afterPosition = readerWithSeek.Position;

            var headerOctetCount = (int)(afterPosition - beforePosition);

            return new(time, tickId, readerWithSeek.ReadOctets((int)octetLength - headerOctetCount));
        }
    }
}