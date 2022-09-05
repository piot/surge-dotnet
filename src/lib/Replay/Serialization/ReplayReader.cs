/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Flood;
using Piot.Raff.Stream;
using Piot.SerializableVersion;
using Piot.SerializableVersion.Serialization;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Replay.Serialization
{
    public readonly struct CompleteStateEntry
    {
        public readonly uint tickId;
        public readonly ulong streamPosition;

        public CompleteStateEntry(uint tickId, ulong streamPosition)
        {
            this.tickId = tickId;
            this.streamPosition = streamPosition;
        }
    }

    public class ReplayReader
    {
        private readonly RaffReader raffReader;
        private readonly IOctetReaderWithSeekAndSkip readerWithSeek;
        private CompleteStateEntry[] completeStateEntries = Array.Empty<CompleteStateEntry>();

        public ReplayReader(IOctetReaderWithSeekAndSkip readerWithSeek)
        {
            this.readerWithSeek = readerWithSeek;
            raffReader = new(readerWithSeek);
            ReadVersionInfo();

            var positionBefore = readerWithSeek.Position;
            ScanForAllCompleteStatePositions();

            readerWithSeek.Seek(positionBefore);
        }

        public SemanticVersion ApplicationVersion { get; private set; }

        public SemanticVersion StateSerializationVersion { get; private set; }

        public TickId FirstCompleteStateTickId => new(completeStateEntries[0].tickId);

        private void ReadVersionInfo()
        {
            var versionPack = raffReader.ReadExpectedChunk(Constants.ReplayIcon, Constants.ReplayName);
            var reader = new OctetReader(versionPack);
            ApplicationVersion = VersionReader.Read(reader);
            StateSerializationVersion = VersionReader.Read(reader);
        }

        private void ScanForAllCompleteStatePositions()
        {
            var tempRaffReader = raffReader;

            List<CompleteStateEntry> entries = new();

            while (true)
            {
                var positionBefore = readerWithSeek.Position;
                var octetLength = tempRaffReader.ReadChunkHeader(out var icon, out var name);
                if (octetLength == 0)
                {
                    break;
                }

                var positionAfterHeader = readerWithSeek.Position;
                if (icon.Value == Constants.CompleteStateIcon.Value)
                {
                    var packType = readerWithSeek.ReadUInt8();
                    if (packType != 0x02)
                    {
                        throw new Exception("wrong");
                    }

                    var tickId = TickIdReader.Read(readerWithSeek);
                    entries.Add(new(tickId.tickId, positionBefore));
                }

                readerWithSeek.Seek(positionAfterHeader + octetLength);
            }

            completeStateEntries = entries.ToArray();
        }

        private CompleteStateEntry FindClosestEntry(TickId tickId)
        {
            var tickIdValue = tickId.tickId;
            if (completeStateEntries.Length == 0)
            {
                throw new Exception("unexpected that no complete states are found");
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

            var previous = completeStateEntries[left - 1];
            if (previous.tickId > tickIdValue)
            {
                throw new Exception("strange state in replay");
            }

            return previous;
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

            if (icon.Value == Constants.CompleteStateIcon.Value)
            {
                readerWithSeek.Seek(readerWithSeek.Position + octetLength);
                return ReadDeltaState();
            }

            var type = readerWithSeek.ReadUInt8();
            var tickIdRange = TickIdRangeReader.Read(readerWithSeek);
            return new(tickIdRange, readerWithSeek.ReadOctets((int)octetLength - 1 - 5));
        }

        private CompleteState ReadCompleteState()
        {
            var octetLength =
                raffReader.ReadExpectedChunkHeader(Constants.CompleteStateIcon, Constants.CompleteStateName);
            var type = readerWithSeek.ReadUInt8();
            var tickId = TickIdReader.Read(readerWithSeek);
            return new(tickId, readerWithSeek.ReadOctets((int)octetLength - 1 - 4));
        }
    }
}