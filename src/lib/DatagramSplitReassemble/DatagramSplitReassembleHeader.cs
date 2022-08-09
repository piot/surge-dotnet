namespace Piot.Surge.DatagramSplitReassemble
{
    public struct DatagramSplitReassembleHeader
    {
        public byte ChannelId; // Not used yet, always 1
        public ulong SequenceId;
        public byte PartId;
        public byte LastPartId;

        public override string ToString()
        {
            return $"[DatagramHeader {ChannelId} {SequenceId} {PartId}/{LastPartId}";
        }
    }
}