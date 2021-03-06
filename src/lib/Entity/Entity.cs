/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public class Entity : IEntity
    {
        private readonly IGeneratedEntity generatedEntity;

        public Entity(EntityId id, IGeneratedEntity generatedEntity)
        {
            Id = id;
            Mode = EntityMode.Created;
            this.generatedEntity = generatedEntity;
        }

        public bool IsAlive => Mode != EntityMode.Deleted;

        public EntityMode Mode { get; set; }

        public EntityId Id { get; }

        ArchetypeId IEntity.ArchetypeId => generatedEntity.ArchetypeId;

        ILogic IEntity.Logic => generatedEntity.Logic;
        IGeneratedEntity IEntity.GeneratedEntity => generatedEntity;


        void IEntitySerializer.Serialize(ulong serializeFlags, IOctetWriter writer)
        {
            generatedEntity.Serialize(serializeFlags, writer);
        }

        void IEntitySerializer.SerializeAll(IOctetWriter writer)
        {
            generatedEntity.SerializeAll(writer);
        }

        void IEntityDeserializer.Deserialize(ulong serializeFlags, IOctetReader reader)
        {
            generatedEntity.Deserialize(serializeFlags, reader);
        }

        void IEntityDeserializer.DeserializeAll(IOctetReader reader)
        {
            generatedEntity.DeserializeAll(reader);
        }

        public void Tick()
        {
            generatedEntity.Tick();
        }

        public void Overwrite()
        {
            generatedEntity.Overwrite();
        }

        public void FireChanges(ulong mask)
        {
            generatedEntity.FireChanges(mask);
        }

        public void FireCreated()
        {
            generatedEntity.FireCreated();
        }

        public void FireDestroyed()
        {
            generatedEntity.FireDestroyed();
        }

        public void UnDoAction(IAction action)
        {
            generatedEntity.UnDoAction(action);
        }

        public void DoAction(IAction action)
        {
            generatedEntity.DoAction(action);
        }

        public IAction[] Actions => generatedEntity.Actions;
    }
}