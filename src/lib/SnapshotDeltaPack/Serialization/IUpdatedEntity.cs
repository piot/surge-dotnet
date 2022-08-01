/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;
using Piot.Flood;
using Surge.SnapshotDeltaPack;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public readonly struct UpdatedEntity : IUpdatedEntity
    {
        private readonly IEntitySerializer serializer;

        public UpdatedEntity(EntityId id, FullChangeMask changeMask, IEntitySerializer serializer)
        {
            Id = id;
            ChangeMask = changeMask;
            this.serializer = serializer;
        }

        public EntityId Id { get; }

        public void Serialize(FullChangeMask serializeMask, IOctetWriter writer)
        {
            serializer.Serialize(serializeMask.mask, writer);
        }

        public FullChangeMask ChangeMask { get; }
    }
}