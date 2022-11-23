/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Raff
{
    public readonly struct ChunkHeader
    {
        public readonly FourCC icon;
        public readonly FourCC name;
        public readonly uint octetLength;

        public ChunkHeader(FourCC icon, FourCC name, uint octetLength)
        {
            this.icon = icon;
            this.name = name;
            this.octetLength = octetLength;
        }
    }
}