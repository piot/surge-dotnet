using System;
using System.Linq;
using System.Text;
using Piot.Flood;

namespace Piot.Raff
{
    public struct FourCC
    {
        public FourCC(byte[] x)
        {
            if (x.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }
            Value = x;
        }

        public byte[] Value { get; }

        public static FourCC Make(string s)
        {
            if (s.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }

            var octets = Encoding.ASCII.GetBytes(s);
            if (octets.Length != 4)
            {
                throw new Exception("FourCC must have exactly four octets");
            }

            return new FourCC(octets);
        }
    }

    public static class Constants
    {
        public static readonly byte[] fileHeader = { 0xF0, 0x9F, 0xA6, 0x8A, 0x52, 0x41, 0x46, 0x46, 0x0a };
    }
    
    public static class Serialize
    {
        public static void WriteHeader(IOctetWriter writer)
        {
            writer.WriteOctets(Constants.fileHeader);
        }

        public static void WriteChunk(IOctetWriter writer, FourCC icon, FourCC name, Memory<byte> octets)
        {
            writer.WriteOctets(icon.Value);
            writer.WriteOctets(name.Value);
            writer.WriteUInt32((uint)octets.Length);
            writer.WriteOctets(octets);
        }
        
        // WriteInternalChunkMarker writes a octet slice to file with an extended header.
        public static void WriteInternalChunkMarker(IOctetWriter writer, FourCC icon)
        {
            writer.WriteOctets(icon.Value);
        }
    }

    public static class DeSerialize
    {
        public static void ReadHeader(IOctetReader reader)
        {
            var readHeader = reader.ReadOctets(9);
            if (!readHeader.SequenceEqual(Constants.fileHeader))
            {
                throw new Exception("illegal header");
            }
        }

        private static FourCC ReadFourCC(IOctetReader reader)
        {
            var octets = reader.ReadOctets(4);
            return new FourCC(octets);
        }

        public static Memory<byte> ReadChunk(IOctetReader reader, out FourCC icon, out FourCC name)
        {
            icon = ReadFourCC(reader);
            name = ReadFourCC(reader);
            var length = reader.ReadUInt32();
            return reader.ReadOctets((int)length);
        }

        public static Memory<byte> ReadExpectedChunk(IOctetReader reader, FourCC expectedIcon, FourCC expectedName)
        {
            var octets = ReadChunk(reader, out var icon, out var name);
            if (!icon.Value.Equals(expectedIcon.Value))
            {
                throw new Exception("not equal");
            }
            if (!name.Value.Equals(expectedName.Value))
            {
                throw new Exception("not equal");
            }

            return octets;
        }

        public static FourCC ReadInternalChunkMarker(IOctetReader reader)
        {
            return ReadFourCC(reader);
        }
    }

} 
