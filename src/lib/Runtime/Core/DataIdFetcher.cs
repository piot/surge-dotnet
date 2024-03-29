/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

namespace Piot.Surge.Core
{
    public static class DataIdFetcher
    {
        public static ushort Id<T>() where T : struct
        {
            return DataIdLookup<T>.value;
        }
    }
}