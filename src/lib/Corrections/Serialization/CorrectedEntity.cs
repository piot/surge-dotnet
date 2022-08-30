/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Flood;
using Piot.Surge.LocalPlayer;

namespace Piot.Surge.Corrections.Serialization
{
    public class CorrectedEntity : ICorrectedEntity
    {
        public CorrectedEntity(EntityId id, LocalPlayerIndex localPlayerIndex, IEntitySerializer correctionSerializer)
        {
            Id = id;
            CorrectionSerializer = correctionSerializer;
            ControlledByLocalPlayerIndex = localPlayerIndex;
        }

        public IEntitySerializer CorrectionSerializer { get; }

        public EntityId Id { get; }

        public LocalPlayerIndex ControlledByLocalPlayerIndex { get; }

        public void SerializePrevious(ulong changedFieldsMask, IOctetWriter writer)
        {
            throw new NotImplementedException();
        }

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