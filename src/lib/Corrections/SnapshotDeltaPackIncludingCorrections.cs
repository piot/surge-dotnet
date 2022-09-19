/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections
{
    /// <summary>
    /// </summary>
    public sealed class SnapshotDeltaPackIncludingCorrections : IOctetSerializable
    {
        public ReadOnlyMemory<byte> deltaSnapshotPackPayload;
        public ReadOnlyMemory<byte> physicsCorrections;
        public TickIdRange tickIdRange;

        public SnapshotDeltaPackIncludingCorrections(TickIdRange tickIdRange,
            ReadOnlySpan<byte> deltaSnapshotPackPayload, ReadOnlySpan<byte> physicsCorrections,
            SnapshotStreamType streamType, SnapshotType snapshotType)
        {
            this.tickIdRange = tickIdRange;
            this.deltaSnapshotPackPayload = deltaSnapshotPackPayload.ToArray();
            this.physicsCorrections = physicsCorrections.ToArray();
            if (physicsCorrections.Length < 1)
            {
                throw new Exception("invalid physics correction");
            }

            StreamType = streamType;
            SnapshotType = snapshotType;
        }

        public SnapshotStreamType StreamType { get; private set; }
        public SnapshotType SnapshotType { get; private set; }

        public void Serialize(IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)deltaSnapshotPackPayload.Length);
            writer.WriteOctets(deltaSnapshotPackPayload.Span);
            writer.WriteUInt16((ushort)physicsCorrections.Length);
            writer.WriteOctets(physicsCorrections.Span);
            TickIdRangeWriter.Write(writer, tickIdRange);
            writer.WriteUInt8((byte)StreamType);
            writer.WriteUInt8((byte)SnapshotType);
        }

        public void Deserialize(IOctetReader reader)
        {
            var packCount = reader.ReadUInt16();
            deltaSnapshotPackPayload = reader.ReadOctets(packCount).ToArray();

            var physicsOctetCount = reader.ReadUInt16();
            physicsCorrections = reader.ReadOctets(physicsOctetCount).ToArray();

            tickIdRange = TickIdRangeReader.Read(reader);
            StreamType = (SnapshotStreamType)reader.ReadUInt8();
            SnapshotType = (SnapshotType)reader.ReadUInt8();
        }

        public override string ToString()
        {
            return
                $"[snapshotDeltaPackIncludingCorrections tickIdRange: {tickIdRange} deltaSnapshotPackPayload length: {deltaSnapshotPackPayload.Length} gamePhysics: {physicsCorrections.Length} type:{SnapshotType} stream:{StreamType}]";
        }
    }
}