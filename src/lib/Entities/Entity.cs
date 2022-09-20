/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.LogicAction;

namespace Piot.Surge.Entities
{
    /// <inheritdoc />
    /// <summary>
    ///     Representation of a network entity
    /// </summary>
    public sealed class Entity : IEntity
    {
        readonly ICompleteEntity completeEntity;

        public Entity(EntityId id, ICompleteEntity completeEntity)
        {
            Id = id;
            Mode = EntityMode.Created;
            this.completeEntity = completeEntity;
        }

        public EntityRollMode RollMode
        {
            get => completeEntity.RollMode;
            set => completeEntity.RollMode = value;
        }

        public IAction[] Actions => completeEntity.Actions;

        public bool IsAlive => Mode != EntityMode.Deleted;

        public EntityMode Mode { get; set; }

        public EntityId Id { get; }

        ArchetypeId IEntity.ArchetypeId => completeEntity.ArchetypeId;

        ILogic IEntity.Logic => completeEntity.Logic;
        ICompleteEntity IEntity.CompleteEntity => completeEntity;


        public void SerializePrevious(ulong changedFieldsMask, IOctetWriter writer)
        {
            completeEntity.SerializePrevious(changedFieldsMask, writer);
        }

        public void SerializeCorrectionState(IOctetWriter writer)
        {
            completeEntity.SerializeCorrectionState(writer);
        }

        /*
        void IEntitySerializer.Serialize(ulong serializeFlags, IOctetWriter writer)
        {
            completeEntity.Serialize(serializeFlags, writer);
        }

        void IEntitySerializer.SerializeAll(IOctetWriter writer)
        {
            completeEntity.SerializeAll(writer);
        }
        ulong IEntityDeserializer.Deserialize(IOctetReader reader)
        {
            return completeEntity.Deserialize(reader);
        }

        void IEntityDeserializer.DeserializeAll(IOctetReader reader)
        {
            completeEntity.DeserializeAll(reader);
        }
        */

        public void DeserializeCorrectionState(IOctetReader reader)
        {
            completeEntity.DeserializeCorrectionState(reader);
        }

        public void Tick()
        {
            completeEntity.Tick();
        }

        public void ClearChanges()
        {
            completeEntity.ClearChanges();
        }

        public void FireChanges(ulong mask)
        {
            completeEntity.FireChanges(mask);
        }


        public void FireDestroyed()
        {
            completeEntity.FireDestroyed();
        }

        public void UnDoAction(IAction action)
        {
            completeEntity.UnDoAction(action);
        }

        public void DoAction(IAction action)
        {
            completeEntity.DoAction(action);
        }

        public void Serialize(ulong changedFieldsMask, IBitWriter writer)
        {
            completeEntity.Serialize(changedFieldsMask, writer);
        }

        public void SerializeAll(IBitWriter writer)
        {
            completeEntity.SerializeAll(writer);
        }

        public void SerializePrevious(ulong changedFieldsMask, IBitWriter writer)
        {
            completeEntity.SerializePrevious(changedFieldsMask, writer);
        }

        public void SerializeCorrectionState(IBitWriter writer)
        {
            completeEntity.SerializeCorrectionState(writer);
        }

        public ulong Deserialize(IBitReader reader)
        {
            return completeEntity.Deserialize(reader);
        }

        public void DeserializeAll(IBitReader reader)
        {
            completeEntity.DeserializeAll(reader);
        }

        public void DeserializeCorrectionState(IBitReader reader)
        {
            completeEntity.DeserializeCorrectionState(reader);
        }

        public override string ToString()
        {
            return $"[entity {Id} {completeEntity.ArchetypeId}]";
        }
    }
}