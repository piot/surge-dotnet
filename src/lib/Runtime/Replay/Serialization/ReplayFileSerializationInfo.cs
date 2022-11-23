/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Raff;

namespace Piot.Surge.Replay.Serialization
{
    public struct ReplayFileSerializationInfo
    {
        public IconAndName FileInfo;
        public IconAndName CompleteStateInfo;
        public IconAndName DeltaStateInfo;

        public ReplayFileSerializationInfo(IconAndName fileInfo,
            IconAndName completeStateInfo,
            IconAndName deltaStateInfo)
        {
            FileInfo = fileInfo;
            CompleteStateInfo = completeStateInfo;
            DeltaStateInfo = deltaStateInfo;
        }
    }
}