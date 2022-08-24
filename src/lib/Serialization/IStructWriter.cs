/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;

namespace Piot.Surge.Serialization
{
    public interface IStructWriter<in T> where T : struct
    {
        public void Write(IOctetWriter writer, T value);
    }
}