using Piot.Flood;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    public static class EntityCountWriter
    {
        public static void WriteEntityCount(uint count, IOctetWriter writer)
        {
            writer.WriteUInt16((ushort)count);
        }
    }
}