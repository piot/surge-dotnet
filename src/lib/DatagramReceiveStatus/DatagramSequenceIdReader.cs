/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.OctetSerialize;

namespace Piot.Surge
{
    public static class DatagramSequenceIdReader
    {
        public static DatagramSequenceId Read(IOctetReader reader)
        {
            return new() { sequenceId = reader.ReadUInt16() };
        }
    }
}