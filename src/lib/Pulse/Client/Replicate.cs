/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.Entities;

namespace Piot.Surge.Pulse.Client
{
    public static class Replicator
    {
        /// <summary>
        ///     Sets the correction state to the target entity.
        ///     Correction State includes both Logic data and optional game specific physics data.
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <param name="correctionPayload"></param>
        public static void Replicate(IEntity targetEntity, ReadOnlySpan<byte> logicPayload,
            ReadOnlySpan<byte> correctionPayload)
        {
            targetEntity.RollMode = EntityRollMode.Replicate;

            var logicPayloadReader = new OctetReader(logicPayload);
            targetEntity.Deserialize(logicPayloadReader);

            var correctionReader = new OctetReader(correctionPayload);
            targetEntity.DeserializeCorrectionState(correctionReader);
        }
    }
}