using Piot.Flood;

namespace Piot.Surge.MonotonicTimeLowerBits
{
    public static class MonotonicTimeLowerBitsWriter
    {
        public static void Write(MonotonicTimeLowerBits lowerBits, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)(lowerBits.lowerBits & 0xffff));
        }
    }
}