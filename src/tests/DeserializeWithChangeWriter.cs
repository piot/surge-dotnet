/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Flood;
using Piot.Surge.GeneratedEntity;

namespace Tests;

public static class DeserializeWithChangeWriter
{
    public static void DeserializeWithChange(IGeneratedEntity generatedEntity, ulong serializeFlags,
        IOctetReader reader, IOctetWriter writerForCurrentValues)
    {
        generatedEntity.Serialize(serializeFlags, writerForCurrentValues);
        generatedEntity.Deserialize(reader);
    }
}