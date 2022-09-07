/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Peter Bjorklund. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using Piot.Surge.Entities;
using Piot.Surge.FastTypeInformation;

namespace Piot.Surge.GeneratedEntity
{
    /// <summary>
    ///     Interfaces that a source code generated entity is implementing.
    /// </summary>
    public interface IGeneratedEntity : IEntitySerializer, IEntityBitSerializer, IEntityDeserializer,
        IEntityBitDeserializer, IEntityChanges, IEntityOverwrite,
        ISimpleLogic, IEntityFireChanges, IEntityActions, IEntityActionsDoUnDo, IAuthoritativeEntityCaptureSnapshot
    {
        public ILogic Logic { get; }

        public EntityRollMode RollMode { get; set; }

        public ArchetypeId ArchetypeId { get; }

        public TypeInformation TypeInformation { get; }
    }
}