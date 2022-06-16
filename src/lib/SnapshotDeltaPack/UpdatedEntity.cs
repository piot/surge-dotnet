/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.ChangeMask;
using Piot.Surge.OctetSerialize;

namespace Surge.SnapshotDeltaPack
{
    public class UpdateEntity : IUpdatedEntity
    {
        private readonly IEntity entity;

        public UpdateEntity(FullChangeMask mask, IEntity entity)
        {
            ChangeMask = mask;
            this.entity = entity;
        }

        public EntityId Id => entity.Id;
        public FullChangeMask ChangeMask { get; }

        public void Serialize(FullChangeMask serializeMask, IOctetWriter writer)
        {
            entity.Serialize(serializeMask.mask, writer);
        }
    }
}