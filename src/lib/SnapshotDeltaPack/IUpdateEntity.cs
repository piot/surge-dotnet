/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge;
using Piot.Surge.ChangeMask;
using Piot.Surge.OctetSerialize;

namespace Surge.SnapshotDeltaPack
{
    public interface IUpdatedEntity : IEntityBase
    {
        public FullChangeMask ChangeMask { get; }

        public void Serialize(FullChangeMask serializeMask, IOctetWriter writer);
    }
}