using System;
using Piot.Surge.Snapshot;

namespace Piot.Surge.SnapshotDeltaPack
{
    /// <summary>
    /// Holds reusable packs for all the entities that has changed from one tick to the next.
    /// </summary>
    public class DeltaSnapshotPackContainer
    {
        readonly SnapshotEntityPackContainer entityUpdateContainer = new();
        readonly SnapshotEntityPackContainer entityCreatedContainer = new();
        readonly SnapshotEntityPackContainer entityDeletedContainer = new();

        public IFeedEntityPackToContainer EntityUpdateContainer => entityUpdateContainer;
        public IReadPackContainer EntityUpdateContainerRead => entityUpdateContainer;
        public IFeedEntityPackToContainer CreatedEntityContainer => entityCreatedContainer;
        public IReadPackContainer CreatedEntityContainerRead => entityCreatedContainer;
        public IFeedEntityPackToContainer DeletedEntityContainer => entityDeletedContainer;
        public IReadPackContainer DeletedEntityContainerRead => entityDeletedContainer;

        public TickId TickId = new ();
    }
}