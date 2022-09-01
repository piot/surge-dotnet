/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.FieldMask;

namespace Piot.Surge.DeltaSnapshot.Pack.Convert
{
    public readonly struct UpdatedEntity : IUpdatedEntity
    {
        public UpdatedEntity(EntityId id, ChangedFieldsMask changeMask, IEntityBothSerializer serializer)
        {
            Id = id;
            ChangeMask = changeMask;
            Serializer = serializer;
        }

        public EntityId Id { get; }

        public IEntityBothSerializer Serializer { get; }


        public ChangedFieldsMask ChangeMask { get; }
    }
}