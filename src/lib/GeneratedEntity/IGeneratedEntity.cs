/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.FastTypeInformation;

namespace Piot.Surge
{
    public interface IGeneratedEntity : IEntitySerializer, IEntityDeserializer, IEntityChanges, IEntityOverwrite,
        ISimpleLogic, IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo
    {
        public ILogic Logic { get; }

        public ArchetypeId ArchetypeId { get; }

        public TypeInformation TypeInformation { get; }
    }
}