/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Flood;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicalInputSerialize
    {
        /// <summary>
        ///     Serializing the game specific inputs to be sent from the client to the authoritative host.
        /// </summary>
        public static void Serialize(IOctetWriter writer, LogicalInput.LogicalInput[] inputs)
        {
            if (inputs.Length > 255)
            {
                throw new Exception("too many inputs to serialize");
            }

            writer.WriteUInt8((byte)inputs.Length);
            if (inputs.Length == 0)
            {
                return;
            }

            const byte InputStreamCount = 1;
            // TODO: Support more streams
            writer.WriteUInt8(InputStreamCount);

            var first = inputs.First();

            TickIdWriter.Write(writer, first.appliedAtTickId);
            var lastFrameId = first.appliedAtTickId;

            var index = 0;
            foreach (var input in inputs)
            {
                if (input.appliedAtTickId.tickId < lastFrameId.tickId)
                {
                    throw new Exception("logical input in wrong order in collection");
                }

                if (index != 0)
                {
                    if (!input.appliedAtTickId.IsImmediateFollowing(lastFrameId))
                    {
                        throw new Exception("logical input wrong delta ");
                    }
                }

                writer.WriteUInt8((byte)input.payload.Length);
                writer.WriteOctets(input.payload);

                lastFrameId = input.appliedAtTickId;
                index++;
            }
        }
    }
}