/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using Piot.Surge.FieldMask;
using Piot.Surge.Tick;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    /// <summary>
    ///     Normally used for building up a <see cref="EntityComponentChangesUnion" />.
    /// </summary>
    public sealed class ComponentFieldMasksMutable
    {
        public ComponentFieldMasksMutable(TickIdRange tickIdRange)
        {
            TickIdRange = tickIdRange;
        }

        public Dictionary<ushort, ulong> FieldMasks { get; } = new();

        public TickIdRange TickIdRange { get; }

        ulong FieldMask(ushort componentTypeIdValue)
        {
            FieldMasks.TryGetValue(componentTypeIdValue, out var mask);
            return mask;
        }

        public void MergeComponentFields(ComponentTypeId componentTypeIdValue, ComponentChangedFieldMask afterMask)
        {
            var before = FieldMask(componentTypeIdValue.id);
            var mergedBits = MaskMerger.MergeBits(before, afterMask.mask);
            FieldMasks[componentTypeIdValue.id] = mergedBits;
        }

        public void SetChangedMask(ComponentTypeId componentTypeId, ulong mask)
        {
            if ((mask & ChangedFieldsMask.DeletedMaskBit) != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mask),
                    "use Deleted() method instead to mark entities as deleted");
            }

            SetMask(componentTypeId, mask);
        }

        void SetMask(ComponentTypeId componentTypeId, ulong mask)
        {
            FieldMasks[componentTypeId.id] = mask;
        }

        public void Deleted(ComponentTypeId componentTypeId)
        {
            SetMask(componentTypeId, ChangedFieldsMask.DeletedMaskBit);
        }
    }
}