/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;

namespace Piot.Surge
{
    public interface IEntityContainer
    {
        IEntity[] AllEntities { get; }
        public IEntity FetchEntity(EntityId entityId);
        public T? FindEntity<T>(EntityId entityId);
        public IEntity? FindEntity(EntityId entityId);
        public T FetchEntity<T>(EntityId entityId);

        void DeleteEntity(EntityId entityId);
        void DeleteEntity(IEntity entity);
    }
}