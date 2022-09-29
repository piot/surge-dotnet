/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Clog;
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
        public static void Rollback(IEntity targetEntity, PredictCollection rollbackStack, TickId tickId, ILog log)
        {
            targetEntity.CompleteEntity.RollMode = EntityRollMode.Rollback;
            if (rollbackStack.TickId < tickId)
            {
                throw new($"suspicious want to rollback to {tickId}, but stack is at {rollbackStack.TickId}");
            }

            while (rollbackStack.TickId >= tickId)
            {
                var predictItem = rollbackStack.GoRollback();
                if (predictItem.tickId.tickId == tickId.tickId)
                {
                    break;
                }

                log.DebugLowLevel("Rolling back {TickId}", predictItem.tickId);
                RollBack(targetEntity, predictItem.undoPack.Span);
            }
        }

        public static void RollBack(IEntity targetEntity, ReadOnlySpan<byte> undoPack)
        {
            var reader = new OctetReader(undoPack);

            targetEntity.CompleteEntity.ClearChanges();
            var changes = targetEntity.CompleteEntity.Deserialize(reader);
            targetEntity.CompleteEntity.FireChanges(changes);
        }
    }
}