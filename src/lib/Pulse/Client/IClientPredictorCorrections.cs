/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;
using Piot.Surge.Tick;

namespace Piot.Surge.Pulse.Client
{
    public interface IClientPredictorCorrections
    {
        public void ReadCorrections(TickId tickId, ReadOnlySpan<byte> snapshotReader);
    }
}