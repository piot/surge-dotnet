/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.ChangeMask;
using Piot.Flood;

namespace Piot.Surge.SnapshotDeltaPack
{
    public interface IUpdatedEntity : IEntityBase
    {
        public ChangedFieldsMask ChangeMask { get; }

        public void Serialize(ChangedFieldsMask serializeMask, IOctetWriter writer);
    }
}