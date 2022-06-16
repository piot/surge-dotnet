/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public class DatagramReceiveStatusWriter
    {
        public void Write(IOctetWriter writer, DatagramReceiveStatus receiveStatus)
        {
            DatagramSequenceIdWriter.Write(writer, receiveStatus.sequenceId);
            DatagramSequenceIdWriter.Write(writer, receiveStatus.waitingForSequenceId);
            DatagramNotificationMaskWriter.Write(writer, receiveStatus.receiveMask);
        }
    }

    public static class DatagramSequenceIdWriter
    {
        public static void Write(IOctetWriter writer, DatagramSequenceId sequenceId)
        {
            writer.WriteUInt32(sequenceId.sequenceId);
        }
    }

    public static class DatagramNotificationMaskWriter
    {
        public static void Write(IOctetWriter writer, DatagramReceiveMask mask)
        {
            writer.WriteUInt64(mask.mask);
        }
    }


    public struct DatagramReceiveStatus
    {
        public DatagramSequenceId sequenceId;
        public DatagramSequenceId waitingForSequenceId;
        public DatagramReceiveMask receiveMask;
    }

    public struct DatagramReceiveMask
    {
        public ulong mask;
    }

    public struct DatagramSequenceId
    {
        public ushort sequenceId;
    }
}