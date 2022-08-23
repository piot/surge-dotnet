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
        /// <summary>
        ///     Deserializes game specific input arriving on the host from the client.
        ///     Currently only one (1) input stream is supported (one local player on each client).
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static LogicalInput.LogicalInput[] Deserialize(IOctetReader reader)
        {
            var inputCount = reader.ReadUInt8();
            if (inputCount == 0)
            {
                return Array.Empty<LogicalInput.LogicalInput>();
            }

            var inputStreamCount = reader.ReadUInt8();
            if (inputStreamCount != 1)
            {
                throw new NotImplementedException("only support for a single input stream for now");
            }

            var array = new LogicalInput.LogicalInput[inputCount];

            var firstFrameId = TickIdReader.Read(reader);
            var lastFrameId = firstFrameId;

            for (var i = 0; i < inputCount; ++i)
            {
                LogicalInput.LogicalInput input = new()
                {
                    appliedAtTickId = new(lastFrameId.tickId)
                };

                var payloadOctetCount = reader.ReadUInt8();
                if (payloadOctetCount > 70)
                {
                    throw new Exception("suspicious input payload octet count");
                }

                input.payload = reader.ReadOctets(payloadOctetCount).ToArray();

                array[i] = input;
            }

            return array;
        }
    }
}