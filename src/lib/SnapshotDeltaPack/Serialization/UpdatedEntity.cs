/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public readonly struct UpdatedEntity : IUpdatedEntity
    {
        public UpdatedEntity(EntityId id, ChangedFieldsMask changeMask, IEntitySerializer serializer)
        {
            Id = id;
            ChangeMask = changeMask;
            Serializer = serializer;
        }

        public EntityId Id { get; }

        public IEntitySerializer Serializer { get; }


        public ChangedFieldsMask ChangeMask { get; }
    }
}