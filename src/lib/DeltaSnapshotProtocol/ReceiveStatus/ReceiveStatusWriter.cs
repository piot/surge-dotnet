using Piot.Flood;
using Piot.Surge.Tick;
using Piot.Surge.Tick.Serialization;

namespace Piot.Surge.DeltaSnapshotProtocol.ReceiveStatus
{
    public static class SnapshotReceiveStatusWriter
    {
        /// <summary>
        ///     Sent from client to host. Client describes the last delta compressed snapshot for TickId it has received
        ///     and how many snapshots it has detected as dropped after that.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="lastReceivedTickId"></param>
        /// <param name="droppedFramesAfterThat"></param>
        public static void Write(IOctetWriter writer, TickId lastReceivedTickId, byte droppedFramesAfterThat)
        {
#if DEBUG
            writer.WriteUInt8(Constants.SnapshotReceiveStatusSync);
#endif

            TickIdWriter.Write(writer, lastReceivedTickId);
            writer.WriteUInt8(droppedFramesAfterThat);
        }
    }
}