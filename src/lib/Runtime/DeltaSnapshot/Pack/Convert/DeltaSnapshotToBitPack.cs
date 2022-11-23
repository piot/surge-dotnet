/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Linq;
using Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.DeltaSnapshot.ComponentFieldMask;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;
using Piot.Surge.FieldMask;
using Piot.Surge.Tick;
using Piot.Surge.Types.Serialization;
using Constants = Piot.Surge.SnapshotProtocol.Constants;

namespace Piot.Surge.DeltaSnapshot.Pack.Convert
{
    public static class DeltaSnapshotToBitPack
    {
        /// <summary>
        ///     Creates a pack from a deltaSnapshotEntityIds delta.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="deltaSnapshotEntityIds"></param>
        /// <returns></returns>
        public static DeltaSnapshotPack ToDeltaSnapshotPack(IDataSender dataSender,
            EventStreamPackItem[] shortLivedEvents, uint[] clientSidePredictedEntities,
            AllEntitiesChangesUnionImmutable allChanges, TickIdRange tickIdRange, IBitWriterWithResult writer, ILog log)
        {
#if DEBUG
            BitMarker.WriteMarker(writer, Constants.SnapshotDeltaSync);
#endif

            if (allChanges.EntitiesComponentChanges.Count == 0)
            {
                log.Notice("No components changed this tick");
            }

            foreach (var changeForOneEntity in allChanges.EntitiesComponentChanges)
            {
                var entityId = changeForOneEntity.Value.entityId;
                log.Debug("considering {EntityId}", entityId);
                var haveWrittenEntityId = false;
                var isClientSidePredicted = clientSidePredictedEntities.Contains(entityId.Value);
                foreach (var componentChange in changeForOneEntity.Value.componentChangesMasks)
                {
                    log.Debug("considering {EntityId} and {ComponentTypeId} {ComponentChange}", entityId, componentChange.Key, componentChange.Value);
                    var componentTypeId = new ComponentTypeId(componentChange.Key);
                    var isLogicComponent = DataInfo.logicComponentTypeIds!.Contains(componentTypeId.id);
                    if (isClientSidePredicted)
                    {
                        log.Debug("this is client side predicted");
                        if (!isLogicComponent)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (isLogicComponent)
                        {
                            log.Debug("skipping logic component {componentTypeId}", componentTypeId.id);

                            continue;
                        }
                    }

                    if (!haveWrittenEntityId)
                    {
                        log.Debug("writing {EntityId}", entityId);
                        EntityIdWriter.Write(writer, entityId);
                        haveWrittenEntityId = true;
                    }

                    log.Debug("considering {ComponentTypeId}", componentTypeId);
                    ComponentTypeIdWriter.Write(writer, componentTypeId);
                    var changedFieldMask = componentChange.Value;
                    var wasDeleted = changedFieldMask == ChangedFieldsMask.DeletedMaskBit;
                    writer.WriteBits(wasDeleted ? 0U : 1U, 1);
                    if (!wasDeleted)
                    {
                        log.Debug("writing {ComponentTypeId {Mask:x4}", componentTypeId, changedFieldMask);
                        dataSender.WriteMask(writer, entityId.Value, componentTypeId.id, changedFieldMask);
                    }
                }

                if (haveWrittenEntityId)
                {
                    ComponentTypeIdWriter.Write(writer, ComponentTypeId.None);
                }
            }

            EntityIdWriter.Write(writer, EntityId.None);

#if DEBUG
            BitMarker.WriteMarker(writer, Constants.SnapshotDeltaEventSync);
#endif
            EventsWriter.Write(shortLivedEvents, writer);

            return new(tickIdRange, writer.Close(out _), SnapshotType.DeltaSnapshot);
        }
    }
}