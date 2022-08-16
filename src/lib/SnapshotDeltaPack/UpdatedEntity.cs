/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.ChangeMask;

namespace Piot.Surge.SnapshotDeltaPack
{
    public class UpdateEntity : IUpdatedEntity
    {
        private readonly IEntity entity;

        public UpdateEntity(ChangedFieldsMask mask, IEntity entity)
        {
            ChangeMask = mask;
            this.entity = entity;
        }

        public EntityId Id => entity.Id;
        public ChangedFieldsMask ChangeMask { get; }

        public IEntitySerializer Serializer => entity;
    }
}