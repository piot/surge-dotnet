/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicalInputDeserialize
    {
        /**
         * Deserializes game specific input arriving on the host from the client.
         */
        public static LogicalInput.LogicalInput[] Deserialize(IOctetReader reader)
        {
            var inputCount = reader.ReadUInt8();
            if (inputCount == 0)
            {
                return Array.Empty<LogicalInput.LogicalInput>();
            }

            var array = new LogicalInput.LogicalInput[inputCount];

            var firstFrameId = SnapshotIdReader.Read(reader);
            var lastFrameId = firstFrameId;

            for (var i = 0; i < inputCount; ++i)
            {
                if (i > 0)
                {
                    var deltaFrameId = reader.ReadUInt8();
                    lastFrameId.frameId += deltaFrameId;
                }


                LogicalInput.LogicalInput input = new()
                {
                    appliedAtSnapshotId =
                    {
                        frameId = lastFrameId.frameId
                    }
                };

                var payloadOctetCount = reader.ReadUInt8();
                input.payload = reader.ReadOctets(payloadOctetCount);

                array[i] = input;
            }

            return array;
        }
    }
}