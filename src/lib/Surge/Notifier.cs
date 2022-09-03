/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.SnapshotDeltaPack.Serialization;

namespace Piot.Surge
{
    public static class Notifier
    {
        /// <summary>
        ///     Fire changes for the fields set in the <see cref="SnapshotDeltaReaderInfoEntity.changeMask" />.
        ///     Also invokes the <see cref="IEntityActionsDoUnDo.DoAction" /> for Actions that have been created
        ///     from the ILogic implementation.
        /// </summary>
        /// <seealso cref="ISimpleLogic.Tick" />
        /// <param name="entities"></param>
        public static void Notify(SnapshotDeltaReaderInfoEntity[] entities)
        {
            foreach (var notifyEntity in entities)
            {
                Notify(notifyEntity.entity);
            }
        }

        public static void Notify(IEntity notifyEntity)
        {
            notifyEntity.FireChanges(notifyEntity.GeneratedEntity.Changes());
            foreach (var action in notifyEntity.Actions)
            {
                notifyEntity.DoAction(action);
            }

            notifyEntity.Overwrite();
        }

        public static void Notify(IEntity[] entities)
        {
            foreach (var notifyEntity in entities)
            {
                Notify(notifyEntity);
            }
        }
    }
}