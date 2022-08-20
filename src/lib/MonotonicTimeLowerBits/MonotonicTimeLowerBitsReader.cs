using Piot.Flood;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class MonotonicTimeLowerBitsReader
    {
        public static MonotonicTimeLowerBits Read(IOctetReader reader)
        {
            return new MonotonicTimeLowerBits { lowerBits = reader.ReadUInt16() };
        }
    }
}