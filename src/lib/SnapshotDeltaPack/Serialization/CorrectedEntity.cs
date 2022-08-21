/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack.Serialization
{
    public class CorrectedEntity : ICorrectedEntity
    {
        public CorrectedEntity(EntityId id, IEntitySerializer correctionSerializer)
        {
            Id = id;
            CorrectionSerializer = correctionSerializer;
        }

        public IEntitySerializer CorrectionSerializer { get; }

        public EntityId Id { get; }

        public void SerializeCorrectionState(IOctetWriter writer)
        {
            CorrectionSerializer.SerializeCorrectionState(writer);
        }

        public void Serialize(ulong changedFieldsMask, IOctetWriter writer)
        {
            throw new NotImplementedException();
        }

        public void SerializeAll(IOctetWriter writer)
        {
            CorrectionSerializer.SerializeAll(writer);
        }
    }
}