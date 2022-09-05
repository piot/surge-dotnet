/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Raff;

namespace Piot.Surge.Replay.Serialization
{
    public static class Constants
    {
        public static FourCC ReplayName = FourCC.Make("rps1");
        public static FourCC ReplayIcon = new(0xF09F8E9E); // Film frames

        public static FourCC CompleteStateName = FourCC.Make("rst1");
        public static FourCC CompleteStateIcon = new(0xF09F96BC); // Picture Frame

        public static FourCC DeltaStateName = FourCC.Make("rds1");
        public static FourCC DeltaStateIcon = new(0xF09FA096); // Right Arrow
    }
}