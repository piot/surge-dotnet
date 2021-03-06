/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Linq;
using Piot.Surge.OctetSerialize;
using Piot.Surge.SnapshotSerialization;

namespace Piot.Surge.LogicalInputSerialization
{
    public static class LogicalInputSerialize
    {
        public static void Serialize(IOctetWriter writer, LogicalInput.LogicalInput[] inputs)
        {
            if (inputs.Length > 255) throw new Exception("too many inputs to serialize");

            writer.WriteUInt8((byte)inputs.Length);
            if (inputs.Length == 0) return;

            var first = inputs.First();

            SnapshotIdWriter.Write(writer, first.appliedAtSnapshotId);
            var lastFrameId = first.appliedAtSnapshotId;

            var index = 0;
            foreach (var input in inputs)
            {
                if (input.appliedAtSnapshotId.frameId < lastFrameId.frameId)
                    throw new Exception("logical input in wrong order in collection");

                var deltaFrameId = input.appliedAtSnapshotId.frameId - lastFrameId.frameId;
                if (index != 0)
                {
                    if (deltaFrameId <= 0) throw new Exception("logical input wrong delta");

                    writer.WriteUInt8((byte)deltaFrameId);
                }

                writer.WriteUInt8((byte)input.payload.Length);
                writer.WriteOctets(input.payload);

                lastFrameId = input.appliedAtSnapshotId;
                index++;
            }
        }
    }
}