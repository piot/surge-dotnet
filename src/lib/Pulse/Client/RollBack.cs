/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public static class RollBacker
    {
        /// <summary>
        ///     Rolling back predicted changes, since a mis-predict has been detected.
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="rollbackStack"></param>
        /// <param name="tickId"></param>
        public static void Rollback(IEntity targetEntity, RollbackStack rollbackStack, TickId tickId)
        {
            targetEntity.CompleteEntity.RollMode = EntityRollMode.Rollback;

            while (rollbackStack.PeekTickId() >= tickId)
            {
                var undoPack = rollbackStack.Pop();

                var reader = new OctetReader(undoPack.payload.Span);

                targetEntity.CompleteEntity.ClearChanges();
                var changes = targetEntity.CompleteEntity.Deserialize(reader);
                targetEntity.CompleteEntity.FireChanges(changes);

                if (undoPack.tickId.tickId == tickId.tickId)
                {
                    break;
                }
            }

            rollbackStack.Clear();
        }
    }
}