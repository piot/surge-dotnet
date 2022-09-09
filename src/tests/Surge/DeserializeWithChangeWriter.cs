/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.Entities;

namespace Tests;

public static class DeserializeWithChangeWriter
{
    public static void DeserializeWithChange(ICompleteEntity completeEntity, ulong serializeFlags,
        IOctetReader reader, IOctetWriter writerForCurrentValues)
    {
        completeEntity.Serialize(serializeFlags, writerForCurrentValues);
        completeEntity.Deserialize(reader);
    }
}