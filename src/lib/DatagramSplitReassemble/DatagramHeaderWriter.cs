using Piot.Flood;

namespace Piot.Surge.DatagramHeader
{
    public static class DatagramHeaderWriter
    {
        public static void Write(DatagramHeader header, IOctetWriter writer)
        {
            writer.WriteUInt8(header.ChannelId);
            writer.WriteUInt64(header.SequenceId);
            writer.WriteUInt8(header.PartId);
            writer.WriteUInt8(header.LastPartId);
        }
    }
}