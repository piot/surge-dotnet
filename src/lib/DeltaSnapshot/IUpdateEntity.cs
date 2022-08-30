/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.FieldMask;

namespace Piot.Surge.DeltaSnapshot
{
    /// <summary>
    ///     The minimal information needed for a updated entity to be serialized into a pack.
    /// </summary>
    public interface IUpdatedEntity : IEntityBase
    {
        public ChangedFieldsMask ChangeMask { get; }

        public IEntitySerializer Serializer { get; }
    }
}