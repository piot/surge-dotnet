/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.ChangeMaskSerialization;
using Piot.Surge.Snapshot;

namespace Piot.Surge.Pulse.Client
{
    public static class RollBacker
    {
        /// <summary>
        ///     Rolling back predicted changes, since a mis-predict has been detected.
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="rollbackQueue"></param>
        /// <param name="correctionsForTickId"></param>
        public static void Rollback(IEntity targetEntity, RollbackQueue rollbackQueue, TickId correctionsForTickId)
        {
            targetEntity.RollMode = EntityRollMode.Rollback;

            for (;;)
            {
                var undoPack = rollbackQueue.Dequeue();

                var reader = new OctetReader(undoPack.payload.Span);

                var flags = ChangedFieldsMaskReader.ReadChangedFieldMask(reader);

                targetEntity.Deserialize(flags.mask, reader);

                if (undoPack.tickId.tickId == correctionsForTickId.tickId)
                {
                    break;
                }
            }
        }
    }
}