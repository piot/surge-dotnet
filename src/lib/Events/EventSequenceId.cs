/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.Event
{
    public readonly struct EventSequenceId
    {
        public readonly ushort sequenceId;

        public EventSequenceId(ushort sequenceId)
        {
            this.sequenceId = sequenceId;
        }

        public EventSequenceId Next()
        {
            return new((ushort)(sequenceId + 1));
        }

        public bool IsValidSuccessor(EventSequenceId value)
        {
            int diff;

            var id = value.sequenceId;
            if (sequenceId < id)
            {
                diff = sequenceId + ushort.MaxValue + 1 - id;
            }
            else
            {
                diff = sequenceId - id;
            }

            if (diff < 0)
            {
                throw new Exception("delta is negative");
            }

            return diff > 0 && diff <= ushort.MaxValue / 2;
        }

        public bool IsEqualOrSuccessor(EventSequenceId value)
        {
            return value.sequenceId == sequenceId || IsValidSuccessor(value);
        }


        public bool Equals(EventSequenceId other)
        {
            return other.sequenceId == sequenceId;
        }

        public override bool Equals(object? obj)
        {
            return obj is not null && base.Equals((EventSequenceId)obj);
        }

        public static bool operator !=(EventSequenceId a, EventSequenceId b)
        {
            return !a.Equals(b);
        }

        public static bool operator ==(EventSequenceId a, EventSequenceId b)
        {
            return a.Equals(b);
        }
    }
}