using Piot.Flood;

namespace Piot.Surge.DatagramType
{
    public static class DatagramTypeWriter
    {
        public static void Write(IOctetWriter writer, DatagramType type)
        {
            writer.WriteUInt8((byte)type);
        }
    }
}