/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.GeneratedEntity;

namespace Piot.Surge
{
    public interface IAuthoritativeEntityContainer
    {
        public IEntity SpawnEntity(IGeneratedEntity generatedEntity);
    }
}