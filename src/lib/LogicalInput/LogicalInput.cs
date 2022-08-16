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


    /// <summary>
    ///     Serialized Game specific input stored in the <see cref="LogicalInput.payload" />.
    /// </summary>
    public struct LogicalInput
    {
        public TickId appliedAtTickId;
        public byte[] payload;

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            var other = (LogicalInput)obj;

            return other.appliedAtTickId.tickId == appliedAtTickId.tickId &&
                   CompareOctets.Compare(other.payload, payload);
        }
    }
}