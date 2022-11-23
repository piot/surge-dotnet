/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Generic;

namespace Piot.Surge.DeltaSnapshot.ComponentFieldMask
{
    public readonly struct ComponentChangedFieldMask
    {
        public ComponentChangedFieldMask(ulong mask)
        {
            this.mask = mask;
        }

        public readonly ulong mask;

        public override string ToString()
        {
            return $"[FieldChangeMask {mask:x4}]";
        }
    }

    /// <summary>
    ///     <see cref="FieldMask" /> for all the entities that changed this tick.
    /// </summary>
    public sealed class ComponentFieldMasks
    {
        public ComponentFieldMasks(Dictionary<ushort, ulong> toFieldMask)
        {
            FieldMasks = toFieldMask;
        }

        public ComponentFieldMasks(ComponentFieldMasksMutable mutable)
        {
            FieldMasks = mutable.FieldMasks;
        }

        public Dictionary<ushort, ulong> FieldMasks { get; }

        public ulong FetchComponentFieldMask(ComponentTypeId componentTypeId)
        {
            return FieldMasks[componentTypeId.id];
        }
    }
}