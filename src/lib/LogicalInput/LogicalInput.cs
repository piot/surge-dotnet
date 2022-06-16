/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Snapshot;

namespace Piot.Surge.LogicalInput
{
    public static class CompareOctets
    {
        public static bool Compare(byte[] a, byte[] b)
        {
            return new ReadOnlySpan<byte>(a).SequenceEqual(b);
        }
    }

    public struct LogicalInput
    {
        public SnapshotId appliedAtSnapshotId;
        public byte[] payload;

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;

            var other = (LogicalInput)obj;

            return other.appliedAtSnapshotId.frameId == appliedAtSnapshotId.frameId &&
                   CompareOctets.Compare(other.payload, payload);
        }
    }
}