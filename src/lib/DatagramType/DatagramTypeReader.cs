using Piot.Flood;

namespace Piot.Surge.DatagramType
{
    public static class DatagramTypeReader
    {
        public static DatagramType Read(IOctetReader reader)
        {
            return (DatagramType) reader.ReadUInt8();
        }
    }
}