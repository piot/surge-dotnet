/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Ecs2;
using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Event;
using Piot.Surge.Event.Serialization;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public struct SerializableComponent
    {
        public ComponentTypeId typeId;
        public HostEntityInfo.IComponentWriter componentWriter;

        public SerializableComponent(ComponentTypeId componentTypeId, HostEntityInfo.IComponentWriter componentWriter)
        {
            typeId = componentTypeId;
            this.componentWriter = componentWriter;
        }
    }

    public interface IEcsContainer
    {
        public ushort[] AllEntities { get; }

        SerializableComponent[] Components(uint entityId);
    }

    public static class CompleteStateBitWriter
    {
        public static ReadOnlySpan<byte> CaptureCompleteSnapshotPack(IDataSender world, uint[] clientSidePredictedEntities,
            EventSequenceId expectedShortLivedEventSequenceId, IBitWriterWithResult bitWriter, ILog log)
        {
#if DEBUG
            BitMarker.WriteMarker(bitWriter, Constants.CompleteSnapshotStartMarker);
#endif

            var entityIds = world.AllEntities();
            if (entityIds.Length == 0)
            {
                log.Debug("Strange, entities to write is zero");
            }

            foreach (var entityIdToSerialize in entityIds)
            {
                var hasWrittenEntityId = false;
                foreach (var componentTypeId in DataInfo.ghostComponentTypeIds!)
                {
                    if (!world.HasComponentTypeId(entityIdToSerialize, (ushort)componentTypeId))
                    {
                        log.Debug("{EntityId} did not contain {ComponentTypeId} so skipping it", entityIdToSerialize, componentTypeId);
                        continue;
                    }

                    if (!hasWrittenEntityId)
                    {
                        log.Debug("Writing {EntityId}", entityIdToSerialize);
                        EntityIdWriter.Write(bitWriter, new((ushort)entityIdToSerialize));
                        hasWrittenEntityId = true;
                    }

                    log.Debug("Writing {ComponentTypeId}", componentTypeId);
                    ComponentTypeIdWriter.Write(bitWriter, new((ushort)componentTypeId));

                    world.WriteFull(bitWriter, entityIdToSerialize, (ushort)componentTypeId);
                }

                if (hasWrittenEntityId)
                {
                    ComponentTypeIdWriter.Write(bitWriter, ComponentTypeId.None);
                }
            }

            EntityIdWriter.Write(bitWriter, EntityId.None);

            EventSequenceIdWriter.Write(bitWriter, expectedShortLivedEventSequenceId);

            EventsWriter.Write(Array.Empty<EventStreamPackItem>(), bitWriter);

            return bitWriter.Close(out _);
        }
    }
}