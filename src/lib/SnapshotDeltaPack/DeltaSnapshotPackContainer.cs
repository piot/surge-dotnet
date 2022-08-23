/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    ///     Holds reusable packs (serialized values) for all the entities that has changed from one tick to the next.
    ///     The values are stored in this container, so they can be fetched again if a delta snapshots needs to be resent.
    ///     It also serves as a small optimization for the current delta snapshot, the field values doesn't have to be
    ///     re-serialized for each client.
    ///     We trade octet based packs instead of bit stream based packs for the performance of putting together a snapshot.
    /// </summary>
    public class DeltaSnapshotPackContainer
    {
        private readonly SnapshotEntityPackContainer entityCorrectionContainer = new();
        private readonly SnapshotEntityPackContainer entityCreatedContainer = new();
        private readonly SnapshotEntityPackContainer entityDeletedContainer = new();
        private readonly SnapshotEntityPackContainer entityUpdateContainer = new();

        public TickId TickId = new();

        public IFeedEntityPackToContainer EntityUpdateContainer => entityUpdateContainer;
        public IReadPackContainer EntityUpdateContainerRead => entityUpdateContainer;
        public IFeedEntityPackToContainer CreatedEntityContainer => entityCreatedContainer;
        public IReadPackContainer CreatedEntityContainerRead => entityCreatedContainer;
        public IFeedEntityPackToContainer DeletedEntityContainer => entityDeletedContainer;
        public IReadPackContainer DeletedEntityContainerRead => entityDeletedContainer;
        public IFeedEntityPackToContainer CorrectionEntityContainer => entityCorrectionContainer;
        public IReadPackContainer CorrectionEntityContainerRead => entityCorrectionContainer;

        public override string ToString()
        {
            return $"[DeltaSnapshotPackContainer {TickId}]";
        }
    }
}