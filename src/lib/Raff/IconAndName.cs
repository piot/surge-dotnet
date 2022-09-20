/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Raff
{
    public struct IconAndName
    {
        public FourCC Icon;
        public FourCC Name;

        public IconAndName(FourCC icon, FourCC name)
        {
            Icon = icon;
            Name = name;
        }
    }
}