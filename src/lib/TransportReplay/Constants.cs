/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Raff;
using Piot.Surge.Replay.Serialization;

namespace Piot.Surge.TransportReplay
{
    public static class Constants
    {
        public static FourCC ReplayName = FourCC.Make("rpt1");
        public static FourCC ReplayIcon = new(0xF09F8E9E); // Film frames

        public static FourCC CompleteStateName = FourCC.Make("rts1");
        public static FourCC CompleteStateIcon = new(0xF09F96BC); // Picture Frame

        public static FourCC DeltaStateName = FourCC.Make("rtd1");
        public static FourCC DeltaStateIcon = new(0xF09FA096); // Right Arrow

        public static ReplayFileSerializationInfo ReplayInfo = new(
            new(ReplayIcon, ReplayName),
            new(CompleteStateIcon, CompleteStateName),
            new(DeltaStateIcon, DeltaStateName)
        );
    }
}