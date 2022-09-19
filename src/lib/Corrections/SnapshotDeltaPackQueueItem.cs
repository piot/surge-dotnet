/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.Corrections
{
    public sealed class SnapshotDeltaPackIncludingCorrectionsItem : IOctetSerializable
    {
        private TickId? previousTickId;

        public SnapshotDeltaPackIncludingCorrectionsItem(SnapshotDeltaPackIncludingCorrections pack,
            TickId? previousTickId)
        {
            this.previousTickId = previousTickId;
            Pack = pack;
        }

        public SnapshotDeltaPackIncludingCorrections Pack { get; }

        public bool IsMergedAndOverlapping
        {
            get
            {
                if (!previousTickId.HasValue)
                {
                    return false;
                }

                return Pack.tickIdRange.Length > 1 && Pack.tickIdRange.Contains(previousTickId.Value);
            }
        }

        public bool IsSkippedAheadSnapshot
        {
            get
            {
                if (!previousTickId.HasValue)
                {
                    return false;
                }

                return !Pack.tickIdRange.Last.IsImmediateFollowing(previousTickId.Value);
            }
        }

        public void Deserialize(IOctetReader reader)
        {
            var isSet = reader.ReadUInt8();
            if (isSet == 0)
            {
                previousTickId = null;
            }
            else
            {
                previousTickId = TickIdReader.Read(reader);
            }
        }

        public void Serialize(IOctetWriter writer)
        {
            if (previousTickId is null)
            {
                writer.WriteUInt8(0);
            }
            else
            {
                writer.WriteUInt8(1);
                TickIdWriter.Write(writer, previousTickId.Value);
            }

            Pack.Serialize(writer);
        }
    }
}