/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.LocalPlayer;

namespace Piot.Surge.SnapshotDeltaPack
{
    public interface ICorrectedEntity : IEntitySerializer, IEntityBase
    {
        LocalPlayerIndex ControlledByLocalPlayerIndex { get; }
    }
}