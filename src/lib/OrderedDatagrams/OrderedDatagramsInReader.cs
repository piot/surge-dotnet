using Piot.Flood;

namespace Piot.Surge.OrderedDatagrams
{
    public static class OrderedDatagramsInReader
    {
        public static OrderedDatagramsIn Read(IOctetReader reader)
        {
            var encounteredId = reader.ReadUInt8();

            return new OrderedDatagramsIn(encounteredId);
        }
    }
}