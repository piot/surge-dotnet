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

                if (entityId.Value == EntityId.NoneValue)
                {
                    break;
                }

                while (true)
                {
                    var componentTypeId = ComponentTypeIdReader.Read(reader);
                    if (componentTypeId.id == ComponentTypeId.NoneValue)
                    {
                        break;
                    }

                    var isAlive = reader.ReadBits(1) != 0;
                    if (isAlive)
                    {
                        DataStreamReceiver.ReceiveUpdate(reader, entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                    }
                    else
                    {
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