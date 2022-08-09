using Piot.Flood;
using Piot.Surge.Snapshot;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public static class SnapshotPackDatagramHeaderWriter
    {
        public static void Write(IOctetWriter writer, TickIdRange tickIdRange, int datagramIndex,
            bool isLastOne)
        {
            var indexMask = (byte)datagramIndex;
            if (isLastOne)
            {
                indexMask |= 0x80;
            }

            TickIdRangeWriter.Write(writer, tickIdRange);
            writer.WriteUInt8(indexMask);
        }
    }
}