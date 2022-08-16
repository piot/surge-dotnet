/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public class SnapshotDeltaIncludedCorrectionPackMemory
    {
        public Memory<byte> memory;
    }
    
    public class SnapshotDeltaWithoutCorrectionPackMemory
    {
        public Memory<byte> memory;
    }
    public static class SnapshotDeltaPacker
    {
        /// <summary>
        ///     Creates a pack payload that is used as part of the datagram
        ///     sent from host to clients.
        /// </summary>
        /// <param name="deletedEntities"></param>
        /// <param name="createdEntities"></param>
        /// <param name="updatedEntities"></param>
        /// <returns></returns>
        public static SnapshotDeltaWithoutCorrectionPackMemory Pack(SnapshotDeltaMemory deltaMemory)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
            SnapshotDeltaWriter.Write(deltaMemory, writer);
            return new SnapshotDeltaWithoutCorrectionPackMemory { memory = writer.Octets };
        }
    }
}