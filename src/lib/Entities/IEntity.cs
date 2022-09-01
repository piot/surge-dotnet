/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.GeneratedEntity;

namespace Piot.Surge.Entities
{
    public enum EntityMode
    {
        Created,
        Normal,
        Deleted
    }

    public interface IEntity : IEntityBothSerializer, IEntityDeserializer, IEntityBitDeserializer, ISimpleLogic,
        IEntityOverwrite,
        IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo
    {
        public EntityMode Mode { get; set; }

        public EntityRollMode RollMode { get; set; }

        public bool IsAlive { get; }

        public EntityId Id { get; }

        public ArchetypeId ArchetypeId { get; }

        public ILogic Logic { get; }

        public IGeneratedEntity GeneratedEntity { get; }
    }
}