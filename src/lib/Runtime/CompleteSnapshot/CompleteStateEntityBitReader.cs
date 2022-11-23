/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Clog;
using Piot.Flood;
using Piot.Surge.Core;
using Piot.Surge.Types.Serialization;

namespace Piot.Surge.CompleteSnapshot
{
    public static class CompleteStateEntityBitReader
    {
        public static void Apply(IBitReader reader,
            IDataReceiver entityGhostContainerWithCreator, ILog log)
        {

#if DEBUG
            //BitMarker.AssertMarker(reader, Constants.SnapshotDeltaSync);
#endif
            while (true)
            {
                var entityId = new EntityId();
                EntityIdReader.Read(reader, ref entityId);

                log.Debug("Complete State Read {EntityId}", entityId.Value);

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

                    DataStreamReceiver.ReceiveNew(reader, entityId.Value, componentTypeId.id, entityGhostContainerWithCreator);
                }
            }
#if DEBUG
//            BitMarker.AssertMarker(reader, Constants.SnapshotDeltaEventSync);
#endif
        }
    }
}