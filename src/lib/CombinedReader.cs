/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge
{
    public struct CombinedPacket
    {
        public DatagramReceiveStatus DatagramReceiveStatus;
        public SerializedSnapshotDeltaPackUnion serializedSnapshots;
    }

    public class CombinedReader
    {
        public CombinedPacket Read(IOctetReader reader)
        {
            return new()
            {
                DatagramReceiveStatus = DatagramReceiveStatusReader.Read(reader),
                serializedSnapshots = SnapshotDeltaUnionReader.Read(reader)
            };
        }
    }
}