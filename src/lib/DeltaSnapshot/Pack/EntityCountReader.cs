using System;
using Piot.Flood;

namespace Piot.Surge.DeltaSnapshot.Pack
{
    public static class EntityCountReader
    {
        public static uint ReadEntityCount(IOctetReader reader)
        {
            var count = reader.ReadUInt16();
            if (count > 128)
            {
                throw new Exception($"suspicious count {count}");
            }

            return count;
        }
    }
}