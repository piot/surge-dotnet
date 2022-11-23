/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.SerializableVersion;

namespace Piot.Surge.Replay.Serialization
{
    public readonly struct ReplayVersionInfo
    {
        public readonly SemanticVersion applicationSemanticVersion;
        public readonly SemanticVersion surgeProtocolSemanticVersion;

        public ReplayVersionInfo(SemanticVersion applicationSemanticVersion,
            SemanticVersion surgeProtocolSemanticVersion)
        {
            this.applicationSemanticVersion = applicationSemanticVersion;
            this.surgeProtocolSemanticVersion = surgeProtocolSemanticVersion;
        }
    }
}