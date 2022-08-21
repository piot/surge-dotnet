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

    public static class SnapshotDeltaPacker
    {
        /// <summary>
        ///     Creates a pack payload that is used as part of the datagram
        ///     sent from host to clients.
        /// </summary>
        /// <param name="deltaMemory"></param>
        /// <returns>The deleted, created and updated memory packs</returns>
        public static SnapshotDeltaIncludedCorrectionPackMemory Pack(SnapshotDeltaMemory deltaMemory)
        {
            var writer = new OctetWriter(Constants.MaxSnapshotOctetSize);
            SnapshotDeltaWriter.Write(deltaMemory, writer);
            return new SnapshotDeltaIncludedCorrectionPackMemory { memory = writer.Octets };
        }
    }
}