/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.DeltaSnapshot.Convert;
using Piot.Surge.DeltaSnapshot.EntityMask;
using Piot.Surge.DeltaSnapshot.Pack;
using Piot.Surge.DeltaSnapshot.Pack.Convert;
using Piot.Surge.DeltaSnapshot.Scan;
using Piot.Surge.Event;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Host
{
    public static class StoreWorldChanges
    {
        public static (EntityMasks, DeltaSnapshotPack) StoreWorldChangesToPackContainer(
            IEntityContainerWithDetectChanges AuthoritativeWorld,
            EventStreamPackQueue ShortLivedEventStream, TickId serverTickId, SnapshotStreamType streamType,
            IOctetWriterWithResult octetWriter, IBitWriterWithResult bitWriter)
        {
            var deltaSnapshotEntityIds = Scanner.Scan(AuthoritativeWorld, serverTickId);
            var eventsThisTick = ShortLivedEventStream.FetchEventsForRange(TickIdRange.FromTickId(serverTickId));
            var deltaSnapshotPack = streamType switch
            {
                SnapshotStreamType.BitStream => DeltaSnapshotToBitPack.ToDeltaSnapshotPack(AuthoritativeWorld,
                    eventsThisTick, deltaSnapshotEntityIds,
                    TickIdRange.FromTickId(deltaSnapshotEntityIds.TickId), bitWriter),

                SnapshotStreamType.OctetStream => DeltaSnapshotToPack.ToDeltaSnapshotPack(AuthoritativeWorld,
                    deltaSnapshotEntityIds, octetWriter)
            };

            ChangeClearer.OverwriteAuthoritative(AuthoritativeWorld);

            return (DeltaSnapshotToEntityMasks.ToEntityMasks(deltaSnapshotEntityIds), deltaSnapshotPack);
        }
    }
}