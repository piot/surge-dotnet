using Piot.Surge.SnnapshotDeltaPack.Serialization;

namespace Piot.Surge
{
    public static class Notifier
    {
        public static void Notify(SnapshotDeltaReaderInfoEntity[] entities)
        {
            foreach (var notifyEntity in entities)
            {
                notifyEntity.entity.FireChanges(notifyEntity.changeMask);
                foreach (var action in notifyEntity.entity.Actions)
                {
                    notifyEntity.entity.DoAction(action);
                }

                notifyEntity.entity.Overwrite();
                
            }
        }
    }
}