using Piot.Flood;

namespace Piot.Surge.DatagramHeader
{
    public static class DatagramHeaderReader
    {
        public static DatagramHeader Read(IOctetReader reader)
        {
            return new DatagramHeader
            {
                ChannelId = reader.ReadUInt8(),
                SequenceId = reader.ReadUInt64(),
                PartId = reader.ReadUInt8(),
                LastPartId = reader.ReadUInt8(),
            };
        }
    }
}