/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public static class DatagramReceiveStatusReader
    {
        public static DatagramReceiveStatus Read(IOctetReader reader)
        {
            var sequenceId = DatagramSequenceIdReader.Read(reader);
            var expectingSequenceId = DatagramSequenceIdReader.Read(reader);
            var receiveMask = DatagramReceiveStatusMaskReader.Read(reader);

            return new()
            {
                receiveMask = receiveMask,
                sequenceId = sequenceId,
                waitingForSequenceId = expectingSequenceId
            };
        }
    }

    public static class DatagramReceiveStatusMaskReader
    {
        public static DatagramReceiveMask Read(IOctetReader reader)
        {
            return new() { mask = reader.ReadUInt64() };
        }
    }
}