/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System;

namespace Piot.Surge.FastTypeInformation
{
    public struct TypeInformationField
    {
        public ulong mask;

        public Type type;

        public TypeInformationFieldName name;
    }
}