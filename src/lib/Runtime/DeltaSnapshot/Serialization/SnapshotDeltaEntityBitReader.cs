/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.SnapshotProtocol;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public static class SnapshotDeltaEntityBitReader
    {
        public static void ReadAndApply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator, bool isOverlappingMergedSnapshot, ILog log)
        {
#if DEBUG
            BitMarker.AssertMarker(reader, Constants.SnapshotDeltaSync);
#endif
            while (true)
            {
                var entityId = new EntityId();
                EntityIdReader.Read(reader, ref entityId);

                log.Debug("Read {EntityId}", entityId.Value);

                if (entityId.Value == EntityId.NoneValue)
                {
                    break;
                }

                while (true)
                {
                    var componentTypeId = ComponentTypeIdReader.Read(reader);
                    if (componentTypeId.id == ComponentTypeId.NoneValue)
                    {
                        log.Debug("End of components");
                        break;
                    }

                    var isAlive = reader.ReadBits(1) != 0;
                    if (isAlive)
                    {
                        log.Debug("Receive component update {EntityId} {ComponentTypeId}", entityId, componentTypeId);
                        DataStreamReceiver.ReceiveUpdate(reader, entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                    }
                    else
                    {
                        log.Debug("Receive component destroy {EntityId} {ComponentTypeId}", entityId, componentTypeId);
                        DataStreamReceiver.ReceiveDestroy(entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                    }
                }
            }
            #if DEBUG
            BitMarker.AssertMarker(reader, Constants.SnapshotDeltaEventSync);
#endif
        }
    }
}