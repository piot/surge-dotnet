using Piot.Flood;
using Piot.Surge.OrderedDatagrams;

namespace OrderedDatagramTests;

public class OrderedDatagramsTests
{
    [Fact]
    public void OrderedDatagrams()
    {
        OrderedDatagramsInChecker sequence = new(new OrderedDatagramsIn(0));
        {
            OctetWriter writer = new(1);
            writer.WriteUInt8(128);
            OctetReader reader = new(writer.Octets);
            Assert.False(sequence.ReadAndCheck(reader));
        }
        {
            OctetWriter writer = new(1);
            writer.WriteUInt8(129);
            OctetReader reader = new(writer.Octets);
            Assert.False(sequence.ReadAndCheck(reader));
        }
    }


    [Fact]
    public void OrderedDatagramsValid()
    {
        OrderedDatagramsInChecker sequence = new();
        {
            OctetWriter writer = new(1);
            writer.WriteUInt8(126);
            OctetReader reader = new(writer.Octets);
            Assert.True(sequence.ReadAndCheck(reader));
        }

        {
            OctetWriter writer = new(1);
            writer.WriteUInt8(127);
            OctetReader reader = new(writer.Octets);
            Assert.True(sequence.ReadAndCheck(reader));
        }

        {
            OctetWriter writer = new(1);
            writer.WriteUInt8(127);
            OctetReader reader = new(writer.Octets);
            Assert.False(sequence.ReadAndCheck(reader));
        }
    }

    [Fact]
    public void OrderedDatagramsWrite()
    {
        OrderedDatagramsOutIncrease sequence = new();
        Assert.Equal(0, sequence.Value.Value);

        for (var i = 0; i < 256; ++i)
        {
            OctetWriter writer = new(1);
            OrderedDatagramsOutWriter.Write(writer, sequence.Value);
            OctetReader reader = new(writer.Octets);
            Assert.Equal(i, reader.ReadUInt8());
            sequence.Increase();
        }

        Assert.Equal(0, sequence.Value.Value);
        OctetWriter writer2 = new(1);
        OrderedDatagramsOutWriter.Write(writer2, sequence.Value);
        sequence.Increase();
        Assert.Equal(1, sequence.Value.Value);
    }

}