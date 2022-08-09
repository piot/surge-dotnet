using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public static class OrderedDatagramsOutWriter
    {
        public static void Write(IOctetWriter writer, OrderedDatagramsOut datagramsOut)
        {
            writer.WriteUInt8(datagramsOut.Value);
        }
    }
}