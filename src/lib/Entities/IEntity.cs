/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Entities
{
    public enum EntityMode
    {
        Created,
        Normal,
        Deleted
    }

    /*
     * : IEntityBothSerializer, IEntityDeserializer, IEntityBitDeserializer, ISimpleLogic,
        IEntityClearChanges,
        IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo
     */

    /// <summary>
    ///     Separate layer for Id, IsAlive, Mode that should not be in the game specific implementation of ICompleteEntity.
    /// </summary>
    public interface IEntity
    {
        public EntityMode Mode { get; set; }

        public bool IsAlive { get; }

        public EntityId Id { get; }

        public ArchetypeId ArchetypeId { get; }

        public ILogic Logic { get; }

        public ICompleteEntity CompleteEntity { get; }
    }
}