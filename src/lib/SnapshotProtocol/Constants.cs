namespace Piot.Surge.SnapshotProtocol
{
    public class Constants
    {
        public const byte SnapshotReceiveStatusSync = 0x18;
        public const byte UnionSync = 0xba;
        public const byte SnapshotDeltaSync = 0xbb;
        public const byte SnapshotDeltaCreatedSync = 0xbc;
        public const byte SnapshotDeltaUpdatedSync = 0x13;
        public const uint MaxDatagramOctetSize = 1200;
        public const uint MaxSnapshotOctetSize = 64 * MaxDatagramOctetSize;
    }
}